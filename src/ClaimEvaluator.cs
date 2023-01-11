using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace src
{
    public class ClaimEvaluator
    {
        public ClaimEvaluator() 
        {

        }

        [FunctionName("ClaimEvaluator")]
        public async Task<IEnumerable<KeyValuePair<string, bool>>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            [Sql("select * from dbo.Rules",
                CommandType = System.Data.CommandType.Text,
                ConnectionStringSetting = "SqlConnectionString")]IEnumerable<Rule> rules,
            Claim claim)
        {
            var outputs = new List<KeyValuePair<string, bool>>();

            //TODO: what rules should I run for this claim?
            var rulesToRun = rules.Where(r => r.RuleType == "Claim");

            //TODO: fan out and execute those rules
            foreach (var rule in rulesToRun)
            {
                var input = new RuleObject() { rule = rule, claim = claim };
                outputs.Add(await context.CallActivityAsync<KeyValuePair<string, bool>>("ClaimEvaluator_EvaluateRule", input));
            }

            //TODO: evaluate results of rules
            var result = outputs.Where(o => o.Value == true);

            //return results
            return result;
        }

        [FunctionName("ClaimEvaluator_EvaluateRule")]
        public async Task<KeyValuePair<string, bool>> SayHello([ActivityTrigger]RuleObject rule, ILogger log)
        {
            //TODO: Route to and invoke logic for provide Rule
            // ex: invoke an HTTP call
            // ex: queue another message somewhere else
            // ex: invoke a SQL SPROC
            KeyValuePair<string, bool> result = new KeyValuePair<string, bool>(rule.rule.RuleName, false);
            if (rule.rule.RuleName == "Rule1")
                result = RulesEngine.RunRule1(rule.claim);
            else if (rule.rule.RuleName == "Rule2")
                result = RulesEngine.RunRule2(rule.claim);
            
            return new KeyValuePair<string, bool>(rule.rule.RuleName, true);
        }

        [FunctionName("ClaimEvaluator_QueueStart")]
        public async Task HttpStart(
            [ServiceBusTrigger("claim-evaluator", Connection = "ServiceBusConnection")] Claim myQueueItem,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("ClaimEvaluator", myQueueItem);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }
    }
}