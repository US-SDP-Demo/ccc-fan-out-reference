using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace ccc_fan_out_reference
{
    public class ClaimProcessor
    {
        private readonly ILogger _logger;

        public ClaimProcessor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<cClaimProcessor>();
        }

        [Function("ClaimProcessor")]
        public async Run(
            [ServiceBusTrigger("myqueue", Connection = "")] string myQueueItem,
            [Sql("select * from dbo.Rules",
                CommandType = System.Data.CommandType.Text,
                ConnectionStringSetting = "SqlConnectionString")]
            IEnumerable<Rules> rules,
            [DurableClient]DurableClientContext context)
        {
            //TODO: retrieve rules
            _logger.LogInformation($"Rule count: {rules.Count()}");
            //TODO: filter rules
            //TOOD: fan-out rule execution
            //TODO: 
        }
    }
}
