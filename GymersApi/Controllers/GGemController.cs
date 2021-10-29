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
    public class GGemController : ControllerBase
    {
        private readonly ILogger<GymersController> _logger;

        private readonly Account _account;

        private readonly Contract _ggemContract;

        public GGemController(IConfiguration configuration, ILogger<GymersController> logger)
        {
            _logger = logger;

            var networkUrl = configuration.GetValue<string>("BlockchainNetWork:Url");
            var networkId = configuration.GetValue<int>("BlockchainNetWork:NetworkId");

            var ggemContractAdressse = configuration.GetValue<string>("GGemContract:Adresse");
            var ggemAbi = configuration.GetValue<string>("GGemContract:Abi");

            _account = new Account(configuration.GetValue<string>("Account:PrivateKey"), networkId);
            var web3 = new Web3(_account, networkUrl);

            _ggemContract = web3.Eth.GetContract(ggemAbi, ggemContractAdressse);
        }

        [HttpGet]
        [Route("balance/{address}")]
        public async Task<long> BalanceOfGem(string address)
        {
            var amount = await _ggemContract.GetFunction("balanceOf").CallAsync<long>(address);

            return amount;
        }

        [HttpGet]
        [Route("transfer/{address}/{amount:long}")]
        public async Task<string> Transfer(string address, long amount)
        {
            var transfer = _ggemContract.GetFunction("transfer");
            var estimatedGas = await transfer.EstimateGasAsync(_account.Address, null, null, address, amount);
            var transactionReceipt = await transfer.SendTransactionAsync(_account.Address, estimatedGas, null, null, address, amount);

            _logger.LogInformation($"Tnx {transactionReceipt} : Transfer {amount} GGem to {address}");

            return transactionReceipt;
        }
    }
}
