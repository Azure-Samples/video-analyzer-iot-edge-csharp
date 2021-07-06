using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp.Hubs
{
    public class EventHub : Hub
    {
        //private readonly CancellationTokenSource cancellationSource;

        //public EventHub()
        //{
        //    cancellationSource = new CancellationTokenSource();
        //}

        //public async Task StartListening()
        //{
        //    //await Groups.AddToGroupAsync(Context.ConnectionId, "groupEventHub");

        //    var connectionString = "Endpoint=sb://iothub-ns-clientweba-12128989-38b9c269d5.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=Q3mrdB/7a+p+wjWfb8ae+zeppdZV9XhHJzojWdUyIsY=;EntityPath=clientwebappiothub";
        //    var eventHubName = "clientwebappiothub";
        //    var consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

        //    var consumer = new EventHubConsumerClient(
        //    consumerGroup,
        //    connectionString,
        //    eventHubName);
        //    var sb = new StringBuilder();
        //    try
        //    {
        //        string firstPartition = (await consumer.GetPartitionIdsAsync(cancellationSource.Token)).Last();
        //        PartitionProperties properties = await consumer.GetPartitionPropertiesAsync(firstPartition, cancellationSource.Token);
        //        EventPosition startingPosition = EventPosition.FromSequenceNumber(properties.LastEnqueuedSequenceNumber);

        //        await foreach (PartitionEvent partitionEvent in consumer.ReadEventsFromPartitionAsync(
        //            firstPartition,
        //            startingPosition,
        //            cancellationSource.Token))
        //        {
        //            //string readFromPartition = partitionEvent.Partition.PartitionId;
        //            //byte[] eventBodyBytes = partitionEvent.Data.EventBody.ToArray();

                   
        //            await Clients.All.SendAsync("ReceivedNewEvent", partitionEvent.Data.EventBody.ToString());
        //        }

                
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        // This is expected if the cancellation token is
        //        // signaled.
        //        await Clients.All.SendAsync("ReceivedNewEvent", "connection_ended");
        //    }
        //    finally
        //    {
        //        await consumer.CloseAsync();
        //    }
        //}

        //public async Task StopListening()
        //{
        //    cancellationSource.Cancel();
        //}
    }
}
