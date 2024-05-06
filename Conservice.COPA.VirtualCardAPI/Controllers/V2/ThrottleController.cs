using System;
using System.Threading;
using System.Threading.Tasks;
using COPA.APIManager.BLL.Clients.Conservice;
using COPA.APIManager.BLL.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Conservice.COPA.VirtualCardAPI.Controllers.V2
{
    /// <summary>
    /// Controller used to allow runtime management of CSI throttling policies.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    public class ThrottleController : ControllerBase
    {
        private readonly ILogger<ThrottleController> _logger;
        private ICSIRateCompliance _csiRateCompliance;

        /// <summary>
        /// Default constructor. Initializes fields through DI
        /// </summary>
        /// <param name="csiRateCompliance"></param>
        /// <param name="logger"></param>
        public ThrottleController(ICSIRateCompliance csiRateCompliance, ILogger<ThrottleController> logger)
        {
            _csiRateCompliance = csiRateCompliance;
            _logger = logger;
        }


        /// <summary>
        /// Sends an API call to CSI to retrieve information for a specified Conservice credit card.
        /// </summary>
        /// <returns>IVirtualCard response object.</returns>
        [Route("csiApiConstraints")]
        [HttpGet]
        [Authorize(Policy = "CanCreateVirtualCard")]
        [ProducesResponseType(typeof(CSIConstraints), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetApiConstraints()
        {
            CSIConstraints constraints = _csiRateCompliance.GetUsageLimits();

            return new OkObjectResult(constraints);
        }

        /// <summary>
        /// Sends an API call to CSI to block the given card
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns>IVirtualCard response object.</returns>
        [Route("csiApiConstraints")]
        [HttpPut]
        [Authorize(Policy = "CanCreateVirtualCard")]
        [ProducesResponseType(typeof(CSIConstraints), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateApiConstraints(CSIConstraints constraints)
        {
            CancellationTokenSource source = new CancellationTokenSource(500);
            try
            {
                CSIConstraints updated = await _csiRateCompliance.UpdateLimitsAsync(constraints, source.Token);

                return new OkObjectResult(updated);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
