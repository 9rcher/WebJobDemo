using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions;

namespace WebJobDemo
{
	public class Functions
	{
		// This function will get triggered/executed when a new message is written 
		// on an Azure Queue called queue.
		/*public static void ProcessQueueMessage([QueueTrigger("wjmq")] string message, TraceWriter log)
		{
			log.Info(message);
		}*/

		public static void ProcessWJMessage (
				[QueueTrigger("wjmq")] string msg,
				ExecutionContext executionContext,
				[Table("wjmt")] CloudTable tb,
				TraceWriter log)
		{
			var messageText = msg;

			log.Info($"receiving: {msg} {executionContext.FunctionName}");
			WJMessageRecord wjmRecord = new WJMessageRecord
			{
				PartitionKey = "wjDemo",
				RowKey = Guid.NewGuid().ToString(),
				MessageBody = msg,
				MessageCreatedDateTime = DateTime.UtcNow
			};

			log.Info($"pre-inserting [cloud table]: {msg} {executionContext.FunctionName}");
			TableOperation insertOperation = TableOperation.Insert(wjmRecord);
			tb.Execute(insertOperation);
			log.Info($"post-inserting [cloud table]: {msg} {executionContext.FunctionName}");

			// throw new FunctionInvocationException("Exception from ProcessWJMessage");
		}

		public static void ErrorMonitor (
			[ErrorTrigger("00:05:00", 2, Throttle = "0:10:00")] TraceFilter filter,
			ExecutionContext executionContext,
			TraceWriter log)
		{
			// slack message body - the attachment of error details
			/* string messageBody = string.Format("");
			// Azure AppSetting - slack url
			// request method: POST
			// payload: application/json
			// 
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, )
			{
				
			}
			
			HttpClient.SendAsync(request); */
			log.Error($"Received exceptions...{executionContext.FunctionName}");
			// log.Error($"{filter.GetDetailedMessage(1)}");
		}
		public class WJMessageRecord : TableEntity
		{
			public DateTime MessageCreatedDateTime { get; set; }
			public string MessageBody { get; set; }
		}
	}
}
