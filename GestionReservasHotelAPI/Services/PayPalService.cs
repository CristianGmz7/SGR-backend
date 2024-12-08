using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.PayPal;
using GestionReservasHotelAPI.Services.Interfaces;
using Newtonsoft.Json;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Payments;
using System.Net.Http.Json;

namespace GestionReservasHotelAPI.Services;

public class PayPalService : IPayPalService
{
    private readonly PayPalEnvironment _environment;
    private readonly PayPalHttpClient _client;
    private readonly string CLIENTID;
    private readonly string SECRET;


    //Credenciales de Sistema Gestion Reservas
    public PayPalService(IConfiguration configuration)
    {
        CLIENTID = configuration.GetValue<string>("PayPal:ClientId");
        SECRET = configuration.GetValue<string>("PayPal:Secret");
        //_environment = new SandboxEnvironment("AQ8oR4e02mPYsn1zpMxoByYtRPMRsKBrlZGEXUrA0-7F24Sa4y_AnfvOv0A6Pjnrn6lHRz5GY8Xx9y6y", "EKKKZWddOA4yQp8d1WeWWi9L8cdsIeUjdjSJOXAkVnb3hMdp4oDdd5EansWAkDH-D3iIMyefqn1iSNCG");
        _environment = new SandboxEnvironment(CLIENTID, SECRET);
        _client = new PayPalHttpClient(_environment);
    }

    //ESTE METODO DE REEMBOLSO APLICA PARA CUANDO: FALLA LA CREACION DE RESERVA, FALLA LA EDICION DE RESERVA O SE ELIMINA UNA RESERVA
    public async Task<ResponseDto<string>> RefundPaymentAsync (RefundOrderDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Reason)) dto.Reason = "Reembolso de reserva";

            var refundRequest = new CapturesRefundRequest(dto.CaptureId);
            refundRequest.RequestBody(new RefundRequest
            {
                NoteToPayer = dto.Reason
            });

            var response = await _client.Execute(refundRequest);

            if ((int)response.StatusCode == 201)
            {
                return new ResponseDto<string>
                {
                    Data = "Reembolso procesado existosamente",
                    Message = "EL reembolso fue exitoso",
                    StatusCode = 201,
                    Status = true
                };
            }

            return new ResponseDto<string>
            {
                Message = "No se pudo procesar el reembolso.",
                StatusCode = (int)response.StatusCode,
                Status = false
            };
        }
        catch(Exception e)
        {
            return new ResponseDto<string>
            {
                Message = $"Error al procesar el reembolso: {e.Message}",
                StatusCode = 500,
                Status = false
            };
        }

    }


    //ESTAS DOS NO SE USAN AL FINAL SE MANEJA CREACION DESDE EL FRONTEND EN EL BACKEND SOLO SE VA A MANEJAR PARTE DE LOS REEMBOLSOS
    //Crear Orden para PayPal
    public async Task<ResponseDto<OrderResponseDto>> CreateOrderAsync(CreateOrderDto dto)
    {
        try
        {
            var orderRequest = new OrderRequest     //configuracion de detalles de compra, incluyendo monto y moneda
            {
                //investigar acerca CheckoutPaymentIntent = "AUTHORIZE"
                CheckoutPaymentIntent = "CAPTURE",          //Intencion del pago CAPTURE   indica que la orden será capturada inmediatamente después de ser aprobada por el pagador.
                //lista de objetos PurchaseUnitRequest cada unidad de compra representa una transacción individual dentro de la orden
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    //En este caso, solo hay una unidad de compra,
                    new PurchaseUnitRequest
                    {
                        //define el monto total de la transacción, incluyendo los detalles del desglose.
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = dto.Currency,        //Especifica la moneda en la que se realizará la transacción

                            Value = dto.Amount.ToString("F2")   //Define el valor total de la transacción convierte el monto a una cadena con dos decimales
                        }
                    }
                }
            };

            var request = new OrdersCreateRequest();        //envia la solicitud de creacion de orden
            request.RequestBody(orderRequest);

            var response = await _client.Execute(request);      //Ejecuta la solicitud y procesa la respuesta.


            //Si la orden se crea exitosamente, devuelve el ID de la orden y el enlace para aprobar el pago.
            if ((int)response.StatusCode == 201)             
            {
                var order = response.Result<Order>();       //se extrae el resultado de la orden de la respuesta HTTP.       
                return new ResponseDto<OrderResponseDto>
                {
                    Data = new OrderResponseDto
                    {
                        OrderId = order.Id,
                        ApprovalLink = order.Links.FirstOrDefault(link => link.Rel == "approve")?.Href      //El enlace de aprobación de la orden, que es el primer enlace en la lista de enlaces (Links) con la relación (Rel) "approve".
                    },
                    Message = "Orden creada exitosamente.",
                    StatusCode = 201,
                    Status = true
                };
            }

            return new ResponseDto<OrderResponseDto>
            {
                Message = "No se pudo crear la orden.",
                StatusCode = (int)response.StatusCode,           
                Status = false
            };
        }
        catch(Exception e)
        {
            return new ResponseDto<OrderResponseDto>
            {
                Message = $"Error al crear la orden: {e.Message}",
                StatusCode = 500,
                Status = false
            };
        }
    }

    //Captura una orden creada previamente
    public async Task<ResponseDto<string>> CaptureOrderAsync(string orderId)
    {
        try
        {
            var request = new OrdersCaptureRequest(orderId);        //Crea una solicitud para capturar la orden especificada por orderId.
            request.RequestBody(new OrderActionRequest());

            var response = await _client.Execute(request);      //Ejecuta la solicitud y procesa la respuesta.

            //Si la captura es exitosa, devuelve un mensaje indicando que el pago fue capturado exitosamente.
            if ((int)response.StatusCode == 201)         
            {
                return new ResponseDto<string>
                {
                    Data = "Pago capturado exitosamente.",
                    Message = "Pago realizado correctamente.",
                    StatusCode = 201,
                    Status = true
                };
            }

            return new ResponseDto<string>
            {
                Message = "No se pudo capturar el pago.",
                StatusCode = (int)response.StatusCode,           
                Status = false
            };
        }
        catch (Exception e)
        {
            return new ResponseDto<string>
            {
                Message = $"Error al capturar el pago: {e.Message}",
                StatusCode = 500,
                Status = false
            };
        }
    }
}
