using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Threading.Tasks;

namespace GymersApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GGimController : ControllerBase
    {
        private readonly ILogger<GymersController> _logger;

        private readonly Account _account;

        private readonly Contract _ggimContract;

        public GGimController(IConfiguration configuration, ILogger<GymersController> logger)
        {
            _logger = logger;

            var networkUrl = configuration.GetValue<string>("BlockchainNetWork:Url");
            var networkId = configuration.GetValue<int>("BlockchainNetWork:NetworkId");

            var ggimContractAdressse = configuration.GetValue<string>("GGimContract:Adresse");
            var ggimAbi = configuration.GetValue<string>("GGimContract:Abi");

            _account = new Account(configuration.GetValue<string>("Account:PrivateKey"), networkId);
            var web3 = new Web3(_account, networkUrl);

            _ggimContract = web3.Eth.GetContract(ggimAbi, ggimContractAdressse);
        }

        [HttpGet]
        [Route("balance/{address}")]
        public async Task<long> BalanceOfGim(string address)
        {
            var amount = await _ggimContract.GetFunction("balanceOf").CallAsync<long>(address);

            return amount;
        }

        [HttpGet]
        [Route("mint/{address}/{amount:long}")]
        public async Task<string> Mint(string address, long amount)
        {
            var mint = _ggimContract.GetFunction("mint");
            var estimatedGas = await mint.EstimateGasAsync(_account.Address, null, null, address, amount);
            var transactionReceipt = await mint.SendTransactionAsync(_account.Address, estimatedGas, null, null, address, amount);

            _logger.LogInformation($"Tnx {transactionReceipt} : Mint {amount} GGim to {address}");

            return transactionReceipt;
        }
    }
}
