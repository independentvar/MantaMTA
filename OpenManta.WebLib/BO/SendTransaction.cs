﻿using System.Collections.Generic;
using System.Linq;
using OpenManta.Core;

namespace OpenManta.WebLib.BO
{
	/// <summary>
	/// Holds a Send Status transaction, this may be for a send or a vMTA.
	/// </summary>
	public class SendTransactionSummary
	{
		/// <summary>
		/// The TransactionStatus type that this object represents.
		/// </summary>
		public TransactionStatus Status { get; set; }

		/// <summary>
		/// The amount of time it has happened.
		/// </summary>
		public long Count { get; set; }

		public SendTransactionSummary() { }
		public SendTransactionSummary(TransactionStatus status, long count)
		{
			Status = status;
			Count = count;
		}
	}

	/// <summary>
	/// Collection of Send Transaction Summaries.
	/// </summary>
	public class SendTransactionSummaryCollection : List<SendTransactionSummary>
	{
		public SendTransactionSummaryCollection() { }
		public SendTransactionSummaryCollection(IEnumerable<SendTransactionSummary> collection) : base(collection) { }

		/// <summary>
		/// The amount of attempts to send that have been made.
		/// </summary>
		private long Attempts
		{
			get
			{
				return this.Sum(ts => ts.Count);
			}
		}

		/// <summary>
		/// The amount of send attempts that have resulted in the remote MX accepting the message.
		/// </summary>
		public long Accepted
		{
			get
			{
				return this.Where(ts => ts.Status == TransactionStatus.Success).Sum(ts => ts.Count);
			}
		}

		/// <summary>
		/// The amount of send attempts that resulted in the remote MX rejecting the message.
		/// </summary>
		public long Rejected
		{
			get
			{
				return this.Where(ts => ts.Status == TransactionStatus.Discarded ||
										ts.Status == TransactionStatus.Failed ||
										ts.Status == TransactionStatus.TimedOut).Sum(ts => ts.Count);
			}
		}

		/// <summary>
		/// The percentage of send attempts that where throttled by MantaMTA.
		/// </summary>
		public double ThrottledPercent
		{
			get
			{
				if (this.Attempts < 1)
					return 0;
				long throttled = this.Where(ts => ts.Status == TransactionStatus.Throttled).Sum(ts => ts.Count);
				return (100d / Attempts) * throttled;
			}
		}

		/// <summary>
		/// The percentage of send attempts that resulted in the remote server temporarily rejecting the message.
		/// </summary>
		public double DeferredPercent
		{
			get
			{
				if (this.Attempts < 1)
					return 0;
				long deferred = this.Where(ts => ts.Status == TransactionStatus.Deferred).Sum(ts => ts.Count);
				return (100d / Attempts) * deferred;
			}
		}
	}
}
