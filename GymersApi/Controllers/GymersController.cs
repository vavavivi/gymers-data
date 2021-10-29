using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymersApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GymersController : ControllerBase
    {
        private readonly ILogger<GymersController> _logger;

        private readonly Account _account;
 
        private readonly Contract _gymersContract;

        public GymersController(IConfiguration configuration, ILogger<GymersController> logger)
        {
            _logger = logger;

            var networkUrl = configuration.GetValue<string>("BlockchainNetWork:Url");
            var networkId = configuration.GetValue<int>("BlockchainNetWork:NetworkId");

            var gymersContractAdressse = configuration.GetValue<string>("GymersContract:Adresse");
            var gymersAbi = configuration.GetValue<string>("GymersContract:Abi");

            _account = new Account(configuration.GetValue<string>("Account:PrivateKey"), networkId);
            var web3 = new Web3(_account, networkUrl);

            _gymersContract = web3.Eth.GetContract(gymersAbi, gymersContractAdressse);
        }

        [HttpGet]
        [Route("balance/{address}")]
        public async Task<long> BalanceOfGymer(string address)
        {
            var amount = await _gymersContract.GetFunction("balanceOf").CallAsync<long>(address);

            return amount;
        }

        [HttpGet]
        [Route("token/{tokenId}")]
        public async Task<string> TokenGymerUrl(long tokenId)
        {
            var tokenUrl = await _gymersContract.GetFunction("tokenURI").CallAsync<string>(tokenId);

            return tokenUrl;
        }

        [HttpGet]
        [Route("tokens/{address}")]
        public async Task<List<long>> GetGymersOf(string address)
        {
            var tokens = await _gymersContract.GetFunction("tokensOfOwner").CallAsync<List<long>>(address);

            return tokens;
        }

        [HttpGet]
        [Route("mint/{tokenId}/{url}")]
        public async Task<string> MintGymer(long tokenId, string url)
        {
            var mint = _gymersContract.GetFunction("mint");
            var estimatedGas = await mint.EstimateGasAsync(_account.Address, null, null, tokenId, url);
            var transactionReceipt = await mint.SendTransactionAsync(_account.Address, estimatedGas, null, tokenId, url);

            _logger.LogInformation($"Tnx {transactionReceipt} : Mint {tokenId} / {url}");

            return transactionReceipt;
        }

        [HttpGet]
        [Route("transfer/{toAddress}/{tokenId}")]
        public async Task<string> TransferFrom(string toAddress, long tokenId)
        {
            var transferFrom = _gymersContract.GetFunction("transferFrom");
            var estimatedGas = await transferFrom.EstimateGasAsync(_account.Address, null, null, _account.Address, toAddress, tokenId);
            var transactionReceipt = await transferFrom.SendTransactionAsync(_account.Address, estimatedGas, null, _account.Address, toAddress, tokenId);

            _logger.LogInformation($"Tnx {transactionReceipt} : TransferTo {toAddress} / {tokenId}");

            return transactionReceipt;
        }
    }
}
