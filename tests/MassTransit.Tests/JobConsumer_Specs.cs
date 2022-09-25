namespace MassTransit.Tests
{
    using System;
    using System.Threading.Tasks;
    using Contracts.JobService;
    using JobConsumerTests;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;


    namespace JobConsumerTests
    {
        using System;


        public interface OddJob
        {
            TimeSpan Duration { get; }
        }


        public class OddJobConsumer :
            IJobConsumer<OddJob>
        {
            public async Task Run(JobContext<OddJob> context)
            {
                await Task.Delay(context.Job.Duration, context.CancellationToken);
            }
        }
    }


    [TestFixture]
    public class JobConsumer_Specs
    {
        [Test]
        public async Task Should_complete_the_job()
        {
            await using var provider = SetupServiceCollection();

            var harness = provider.GetTestHarness();

            await harness.Start();

            var jobId = NewId.NextGuid();

            IRequestClient<SubmitJob<OddJob>> client = harness.GetRequestClient<SubmitJob<OddJob>>();

            Response<JobSubmissionAccepted> response = await client.GetResponse<JobSubmissionAccepted>(new
            {
                JobId = jobId,
                Job = new { Duration = TimeSpan.FromSeconds(1) }
            });

            Assert.That(response.Message.JobId, Is.EqualTo(jobId));

            Assert.That(await harness.Published.Any<JobSubmitted>(), Is.True);
            Assert.That(await harness.Published.Any<JobStarted>(), Is.True);

            Assert.That(await harness.Published.Any<JobCompleted>(), Is.True);
            Assert.That(await harness.Published.Any<JobCompleted<OddJob>>(), Is.True);
        }

        [Test]
        public async Task Should_cancel_the_job()
        {
            await using var provider = SetupServiceCollection();

            var harness = provider.GetTestHarness();

            await harness.Start();

            var jobId = NewId.NextGuid();

            IRequestClient<SubmitJob<OddJob>> client = harness.GetRequestClient<SubmitJob<OddJob>>();

            Response<JobSubmissionAccepted> response = await client.GetResponse<JobSubmissionAccepted>(new
            {
                JobId = jobId,
                Job = new { Duration = TimeSpan.FromSeconds(10) }
            });

            Assert.That(response.Message.JobId, Is.EqualTo(jobId));

            Assert.That(await harness.Published.Any<JobSubmitted>(), Is.True);
            Assert.That(await harness.Published.Any<JobStarted>(), Is.True);

            await harness.Bus.Publish<CancelJob>(new { JobId = jobId });

            Assert.That(await harness.Published.Any<JobCanceled>(), Is.True);
        }

        [Test]
        public async Task Should_cancel_the_job_while_waiting()
        {
            await using var provider = SetupServiceCollection();

            var harness = provider.GetTestHarness();

            await harness.Start();

            var previousJobId = NewId.NextGuid();

            var jobId = NewId.NextGuid();

            IRequestClient<SubmitJob<OddJob>> client = harness.GetRequestClient<SubmitJob<OddJob>>();

            Response<JobSubmissionAccepted> response = await client.GetResponse<JobSubmissionAccepted>(new
            {
                JobId = previousJobId,
                Job = new { Duration = TimeSpan.FromSeconds(10) }
            });

            Assert.That(await harness.Published.Any<JobStarted>(x => x.Context.Message.JobId == previousJobId), Is.True);

            response = await client.GetResponse<JobSubmissionAccepted>(new
            {
                JobId = jobId,
                Job = new { Duration = TimeSpan.FromSeconds(10) }
            });

            Assert.That(await harness.Published.Any<JobSubmitted>(x => x.Context.Message.JobId == jobId), Is.True);

            Assert.That(response.Message.JobId, Is.EqualTo(jobId));

            Assert.That(await harness.Sent.Any<JobSlotUnavailable>(x => x.Context.Message.JobId == jobId), Is.True);

            await harness.Bus.Publish<CancelJob>(new { JobId = jobId });

            Assert.That(await harness.Published.Any<JobCanceled>(x => x.Context.Message.JobId == jobId), Is.True);
        }

        static ServiceProvider SetupServiceCollection()
        {
            var provider = new ServiceCollection()
                .AddMassTransitTestHarness(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();

                    x.AddConsumer<OddJobConsumer>();

                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.UseDelayedMessageScheduler();

                        var options = new ServiceInstanceOptions()
                            .SetEndpointNameFormatter(context.GetService<IEndpointNameFormatter>() ??
                                DefaultEndpointNameFormatter.Instance);

                        cfg.ServiceInstance(options, instance =>
                        {
                            instance.ConfigureJobServiceEndpoints();

                            instance.ConfigureEndpoints(context, f => f.Include<OddJobConsumer>());
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                })
                .BuildServiceProvider(true);

            var harness = provider.GetTestHarness();

            harness.TestInactivityTimeout = TimeSpan.FromSeconds(10);

            return provider;
        }
    }
}
