using Conservice.COPA.VirtualCardAPI.Models.Request;
using COPA.APIManager.BLL.Clients.Conservice;
using COPA.APIManager.BOL;
using COPA.APIManager.BOL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Conservice.Platform.ProcessResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Payment.Portal.CL.Utilities.Interfaces;
using Microsoft.Extensions.Logging;

namespace Conservice.COPA.VirtualCardAPI.Controllers.V2
{
    /// <summary>
    /// Controller used to handle CSI api calls.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    public class ConserviceController : ControllerBase
    {
        private readonly ILogger<ConserviceController> _logger;
        private readonly ConserviceApiManager _conserviceApiManager;
        private readonly IClaimsPrincipalService _claimsPrincipalService;

        /// <summary>
        /// Default constructor. Initializes fields through DI
        /// </summary>
        /// <param name="conserviceApiManager">Instance used to communicate with the client VirtualCard API</param>
        /// <param name="claimsPrincipalService"></param>
        /// <param name="logger"></param>
        public ConserviceController(ConserviceApiManager conserviceApiManager, IClaimsPrincipalService claimsPrincipalService, ILogger<ConserviceController> logger)
        {
            _conserviceApiManager = conserviceApiManager;
            _claimsPrincipalService = claimsPrincipalService;
            _logger = logger;
        }

        /// <summary>
        /// Interacts with the CSI VCard API to create a virtual card.
        /// </summary>
        /// <param name="request">Object representation of the JSON body of the request.</param>
        /// <returns>IVirtualCard response object.</returns>
        [Route("Create")]
        [HttpPost]
        [Authorize(Policy = "CanCreateVirtualCard")]
        [ProducesResponseType(typeof(IVirtualCard), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCard(CreateCardRequest request)
        {
            string authorizedParty = _claimsPrincipalService.GetAuthorizedParty();
            try
            {
                _logger.Log(LogLevel.Trace, "Card request received");
                CSICardRequest cardRequest = MapCardRequest(request);

                _logger.Log(LogLevel.Trace, "Card request sent to CSI");
                TypeResult<IVirtualCard, CSIErrorResponse> response = await _conserviceApiManager.CreateConserviceCard(cardRequest, authorizedParty);

                _logger.Log(LogLevel.Trace, "Card response received");

                return response.CreateActionResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Sends an API call to CSI to retrieve information for a specified Conservice credit card.
        /// </summary>
        /// <param name="clientID">The unique identifier associated to a Conservice client.</param>
        /// <param name="cardID">The id of the card we are trying to receive information for.</param>
        /// <returns>IVirtualCard response object.</returns>
        [Route("{clientID}/{cardID:long}")]
        [HttpGet]
        [Authorize(Policy = "CanViewVirtualCard")]
        [ProducesResponseType(typeof(IVirtualCard), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCardInformation(string clientID, string cardID)
        {
            string authorizedParty = _claimsPrincipalService.GetAuthorizedParty();
            try
            {
                _logger.Log(LogLevel.Trace, $"Conservice request sent for {cardID}");
                TypeResult<IVirtualCard, CSIErrorResponse> response = await _conserviceApiManager.GetConserviceCardInfo(clientID, cardID, authorizedParty);
                _logger.Log(LogLevel.Trace, $"Conservice card received for {cardID}");

                return response.CreateActionResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Sends an API call to CSI to block the given card
        /// </summary>
        /// <param name="clientID">The unique identifier associated to a Conservice client.</param>
        /// <param name="cardID">The id of the card we are trying to receive information for.</param>
        /// <returns>IVirtualCard response object.</returns>
        [Route("Block/{clientID}/{cardID:long}")]
        [HttpPut]
        [Authorize(Policy = "CanUpdateVirtualCard")]
        [ProducesResponseType(typeof(IVirtualCard), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BlockCard(string clientID, string cardID)
        {
            string authorizedParty = _claimsPrincipalService.GetAuthorizedParty();
            try
            {
                _logger.Log(LogLevel.Trace, $"Block Conservice card request sent for {cardID}");
                TypeResult<IVirtualCard, CSIErrorResponse> response = await _conserviceApiManager.BlockCard(clientID, cardID, authorizedParty);
                _logger.Log(LogLevel.Trace, $"Blocked card response received for {cardID}");

                return response.CreateActionResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Sends an API call to CSI to update a card's information or allowed amount for a given id
        /// </summary>
        /// <param name="request">Contains card information and what amount it should be updated to</param>
        /// <returns>IVirtual Card response</returns>
        [Route("Update")]
        [HttpPost]
        [Authorize(Policy = "CanUpdateVirtualCard")]
        [ProducesResponseType(typeof(IVirtualCard), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCardInformation(UpdateCardRequest request)
        {
            string authorizedParty = _claimsPrincipalService.GetAuthorizedParty();
            try
            {
                _logger.Log(LogLevel.Trace, "Update Card request received");
                CSIUpdateRequest cardRequest = MapCardUpdateRequest(request);

                _logger.Log(LogLevel.Trace, "Card update request sent to CSI");
                TypeResult<IVirtualCard, CSIErrorResponse> response = await _conserviceApiManager.UpdateConserviceCard(cardRequest, authorizedParty);

                _logger.Log(LogLevel.Trace, "Card response received");

                return response.CreateActionResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private CSICardRequest MapCardRequest(CreateCardRequest request)
        {
            return new CSICardRequest
            {
                ClientID = request.ClientID,
                Amount = request.Amount,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PoNumber = request.Pin,
                SupplierID = request.SupplierID,
                BinRotation = request.BinRotation,
            };
        }

        private CSIUpdateRequest MapCardUpdateRequest(UpdateCardRequest request)
        {
            return new CSIUpdateRequest
            {
                ClientID = request.ClientID,
                ID = request.VirtualCardID,
                Amount = request.Amount,
                FirstName = request.FirstName,
                LastName = request.LastName
            };
        }
    }

    public static class VCardResponseExtension
    {
        public static ActionResult CreateActionResult(this TypeResult<IVirtualCard, CSIErrorResponse> response)
        {
            if (response.Succeeded)
            {
                return new OkObjectResult(response.Value);
            }

            // should we pass on the raw response body from CSI? should we do something else?
            return new ObjectResult(response.Failure.RawBody)
            {
                StatusCode = (int)response.Failure.StatusCode
            };
        }
    }
}
