﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Store.DAL.Repos.Interfaces;
using Store.Models.Entities;


namespace Store.Service.Controllers
{
    
    [Route("Api/[controller]/{customerId}")]
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartRepo _shoppingCart;

        public ShoppingCartController(IShoppingCartRepo shoppingCart)
        {
            _shoppingCart = shoppingCart;
        }

        [HttpGet("{productId}")]
        public IActionResult GetShoppingCartRecord(int customerId, int productId)
        {
            if (Int32.Parse(User.FindFirst(ClaimTypes.Authentication).Value) != customerId) return NotFound();

            var cartRecord = _shoppingCart.GetShoppingCartRecord(customerId, productId);

            return (cartRecord == null) ? (IActionResult)NotFound() : Ok(cartRecord);
        }
        [HttpGet(Name = "GetShoppingCart")]
        public IActionResult GetShoppingCart(int customerId)
        {
            if (Int32.Parse(User.FindFirst(ClaimTypes.Authentication).Value) != customerId) return NotFound();

            var cartrecord = _shoppingCart.GetShoppingCartRecords(customerId);

            return (!cartrecord.Any()) ? (IActionResult)NotFound() : Ok(cartrecord); 
        }
        [HttpPost]
        public IActionResult CreateCart([FromBody] ShoppingCartRecord cartRecord ,int customerId)
        {
            if (Int32.Parse(User.FindFirst(ClaimTypes.Authentication).Value) != customerId) return NotFound();

            if (cartRecord == null || !ModelState.IsValid) {

                return BadRequest(ModelState);
            }

            if (_shoppingCart.Add(cartRecord) == 0)
            {
                return BadRequest();
            }

            cartRecord.DateCreated = DateTime.UtcNow;
            cartRecord.CustomerId = customerId;

            _shoppingCart.SaveChanges();

            return CreatedAtAction("GetShoppingCart", cartRecord);

        }

        [HttpPut("{shoppingCartRecordId}")]
        public IActionResult UpdateCart (int customerId, int shoppingCartRecordId, [FromBody]ShoppingCartRecord shoppingCart)
        {
            if (Int32.Parse(User.FindFirst(ClaimTypes.Authentication).Value) != customerId) return NotFound();

            if (shoppingCart == null || shoppingCartRecordId != shoppingCart.Id || !ModelState.IsValid)
            {
                return BadRequest();
            }
            shoppingCart.DateCreated = DateTime.Now;
            _shoppingCart.Update(shoppingCart);

            return CreatedAtRoute("GetShoppingCart", new {  customerId });

        }
        [HttpDelete("{shoppingCartRecordId}/{timeStamp}")]
        public IActionResult Delete(int customerId, int shoppingCartRecordId, string timeStamp)
        {
            if (Int32.Parse(User.FindFirst(ClaimTypes.Authentication).Value) != customerId) return NotFound();

            if (!timeStamp.StartsWith("\""))
            {
                timeStamp = $"\"{timeStamp}\"";
            }
            var ts = JsonConvert.DeserializeObject<byte[]>(timeStamp);
            _shoppingCart.Delete(shoppingCartRecordId, ts);
            return NoContent();
        }
        [HttpPost("buy")] //required even if method name starts with "Post"
        public IActionResult Purchase(int customerId, [FromBody] Customer customer)
        {

            if (customer == null || customer.Id != customerId || !ModelState.IsValid)
            {
                return BadRequest();
            }
            int orderId;
            orderId = _shoppingCart.Purchase(customerId);
            //Location: http://localhost:8477/api/Orders/0/1
            return CreatedAtRoute("GetOrderDetails", routeValues: new {  customerId,  orderId }, value: orderId);
        }
    }
}
