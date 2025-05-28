﻿using API.Extensions;
using API.SignalR;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stripe;

namespace API.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentsController(IPaymentService paymentService, 
    IUnitOfWork unit, ILogger<PaymentsController> logger, 
    IConfiguration config, IHubContext<NotificationHub> hubContext) : BaseApiController
{
    private readonly string _whSecret = config["StripeSettings:WhSecret"]!;

    [Authorize]
    [HttpPost("{cartId}")]
    public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cartId)
    {
        var cart = await paymentService.CreateOrUpdatePaymentIntent(cartId);

        if (cart == null) return BadRequest("Problem with your cart");

        return Ok(cart);
    }

    [HttpGet("delivery-methods")]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
    {
        return Ok(await unit.Repository<DeliveryMethod>().ListAllAsync());
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
         logger.LogInformation("StripeWebhook endpoint called 111111");
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = ConstructStripeEvent(json);

            if (stripeEvent.Data.Object is not PaymentIntent intent)
            {
                return BadRequest("Invalid event data");
            }

            await HandlePaymentIntentSucceeded(intent);

            return Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook error");
            return StatusCode(StatusCodes.Status500InternalServerError,  "Webhook error");
        }
        catch (Exception ex)
        {
               logger.LogError(ex, "An unexpected error occurred: {Message} {StackTrace}", ex.Message, ex.StackTrace);
               return StatusCode(StatusCodes.Status500InternalServerError,  "An unexpected error occurred");
        }
    }

    private async Task HandlePaymentIntentSucceeded(PaymentIntent intent)
    {
        logger.LogInformation("HandlePaymentIntentSucceeded called for PaymentIntentId={PaymentIntentId}, Status={Status}", intent.Id, intent.Status);
        if (intent.Status == "succeeded") 
        {
            var spec = new OrderSpecification(intent.Id, true);

            var order = await unit.Repository<Core.Entities.OrderAggregate.Order>().GetEntityWithSpec(spec)
                ?? throw new Exception("Order not found");

            if ((long)order.GetTotal() * 100 != intent.Amount)
            {
                order.Status = OrderStatus.PaymentMismatch;
            } 
            else
            {
                order.Status = OrderStatus.PaymentReceived;
            }

            await unit.Complete();

            var connectionId = NotificationHub.GetConnectionIdByEmail(order.BuyerEmail);

            if (!string.IsNullOrEmpty(connectionId))
            {
                await hubContext.Clients.Client(connectionId)
                    .SendAsync("OrderCompleteNotification", order.ToDto());
            }
        }
    }

    private Event ConstructStripeEvent(string json)
{
    try
    {
        return EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            _whSecret,
            throwOnApiVersionMismatch: false // 允许 API 版本不一致
        );
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to construct stripe event");
        throw new StripeException("Invalid signature");
    }
}
}