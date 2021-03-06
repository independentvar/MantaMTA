﻿using OpenManta.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace OpenManta.Framework.RabbitMq
{
	public static class RabbitMqInboundStagingHandler
	{
		public const int STAGING_DEQUEUE_THREADS = 25;
		public static int _StartedThreads = 0;

		public static void Start()
		{
			for (int i = 0; i < STAGING_DEQUEUE_THREADS; i++)
			{
                Task.Factory.StartNew(HandleDequeue, TaskCreationOptions.LongRunning);
				/*Thread t = new Thread(new ThreadStart(HandleDequeue));
				t.IsBackground = true;
				t.Start();*/
			}
		}

		private static async Task HandleDequeue()
		{
			if (_StartedThreads >= STAGING_DEQUEUE_THREADS)
				return;

			_StartedThreads++;

			while(true)
			{
				BasicDeliverEventArgs ea = RabbitMq.RabbitMqManager.Dequeue(RabbitMqManager.RabbitMqQueue.InboundStaging, 1, 100).FirstOrDefault();
				if(ea == null)
				{
                    await Task.Delay(1000);
					continue;
				}

                MtaQueuedMessage qmsg = await Serialisation.Deserialise<MtaQueuedMessage>(ea.Body);
                MtaMessage msg = new MtaMessage
                {
                    ID = qmsg.ID,
                    InternalSendID = qmsg.InternalSendID,
                    MailFrom = qmsg.MailFrom,
                    RcptTo = qmsg.RcptTo,
                    VirtualMTAGroupID = qmsg.VirtualMTAGroupID
                };

				await RabbitMqManager.Publish(msg, RabbitMqManager.RabbitMqQueue.Inbound, true, (RabbitMqPriority)qmsg.RabbitMqPriority);
                await RabbitMqManager.Publish(qmsg, RabbitMqManager.RabbitMqQueue.OutboundWaiting, true, (RabbitMqPriority)qmsg.RabbitMqPriority);
				RabbitMqManager.Ack(RabbitMqManager.RabbitMqQueue.InboundStaging, ea.DeliveryTag, false);
			}
		}
	}
}
