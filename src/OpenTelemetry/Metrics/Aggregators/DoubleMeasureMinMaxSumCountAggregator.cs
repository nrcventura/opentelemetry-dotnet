// <copyright file="DoubleMeasureMinMaxSumCountAggregator.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Threading;
using OpenTelemetry.Metrics.Export;

namespace OpenTelemetry.Metrics.Aggregators
{
    /// <summary>
    /// Aggregator which calculates summary (Min,Max,Sum,Count) from measures.
    /// </summary>
    [Obsolete("Metrics API/SDK is not recommended for production. See https://github.com/open-telemetry/opentelemetry-dotnet/issues/1501 for more information on metrics support.")]
    public class DoubleMeasureMinMaxSumCountAggregator : Aggregator<double>
    {
        private DoubleSummary summary = new DoubleSummary();
        private DoubleSummary checkPoint = new DoubleSummary();
        private object updateLock = new object();

        /// <inheritdoc/>
        public override void Checkpoint()
        {
            base.Checkpoint();
            this.checkPoint = Interlocked.Exchange(ref this.summary, new DoubleSummary());
        }

        /// <inheritdoc/>
        public override AggregationType GetAggregationType()
        {
            return AggregationType.DoubleSummary;
        }

        /// <inheritdoc/>
        public override MetricData ToMetricData()
        {
            return new DoubleSummaryData
            {
                Count = this.checkPoint.Count,
                StartTimestamp = new DateTime(this.GetLastStartTimestamp().Ticks),
                Sum = this.checkPoint.Sum,
                Min = this.checkPoint.Min,
                Max = this.checkPoint.Max,
                Timestamp = new DateTime(this.GetLastEndTimestamp().Ticks),
            };
        }

        /// <inheritdoc/>
        public override void Update(double value)
        {
            lock (this.updateLock)
            {
                base.Update(value);
                this.summary.Count++;
                this.summary.Sum += value;
                this.summary.Max = Math.Max(this.summary.Max, value);
                this.summary.Min = Math.Min(this.summary.Min, value);
            }
        }

        private class DoubleSummary
        {
            public long Count;
            public double Min;
            public double Max;
            public double Sum;

            public DoubleSummary()
            {
                this.Min = double.MaxValue;
                this.Max = double.MinValue;
            }
        }
    }
}
