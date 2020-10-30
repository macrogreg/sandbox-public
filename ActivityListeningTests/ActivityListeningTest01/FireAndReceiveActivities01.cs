using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ActivityListeningTest01
{
    public class FireAndReceiveActivities01
    {
        public const string TestActivitySourceName = "Test-Activity-Source";

        public void Exec()
        {
            Console.WriteLine();

            ActivitySource activitySource = new ActivitySource(TestActivitySourceName);
            Console.WriteLine($"Created ActivitySource. Name={QuoteOrSpellNull(activitySource.Name)}.");

            Console.WriteLine();

            {
                const string activityMoniker = "Activity 1";
                Console.WriteLine($"About to create activity \"{activityMoniker}\" (using ActivitySource).");

                Activity a = activitySource.StartActivity(activityMoniker, ActivityKind.Server);
                Console.WriteLine($"Created activity (\"{activityMoniker}\"):"
                                + $" Activity is non-null: {a != null},"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");

                a?.Dispose();
                Console.WriteLine($"Stopped activity (\"{activityMoniker}\"):"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");
            }

            Console.WriteLine();
            ConfigureDiagnosticListening();
            Console.WriteLine();

            {
                const string activityMoniker = "Activity 2";
                Console.WriteLine($"About to create activity \"{activityMoniker}\" (using ActivitySource).");

                Activity a = activitySource.StartActivity(activityMoniker, ActivityKind.Producer);
                Console.WriteLine($"Created activity (\"{activityMoniker}\"):"
                                + $" Activity is non-null: {a != null},"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");

                a?.Dispose();
                Console.WriteLine($"Stopped activity (\"{activityMoniker}\"):"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");
            }

            Console.WriteLine();
            Console.WriteLine("Starting to configure DiagnosticSource");
            DiagnosticSource diagnosticSource = new DiagnosticListener("My.Test.Diagnostic.Source");
            Console.WriteLine("Finished configuring DiagnosticSource");
            Console.WriteLine();

            {
                const string activityMoniker = "Activity 3";
                Console.WriteLine($"About to create activity \"{activityMoniker}\" (using ActivitySource, passing A. to DiagnosticSource).");

                Activity a = activitySource.StartActivity(activityMoniker, ActivityKind.Consumer);
                Console.WriteLine($"Created activity (\"{activityMoniker}\"):"
                                + $" Activity is non-null: {a != null},"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)}.");

                try
                {
                    diagnosticSource.StartActivity(a, "Start-Activity-Args-Object");
                    Console.WriteLine($"Started activity (\"{activityMoniker}\"):"
                                    + $" Activity is non-null: {a != null},"
                                    + $" Id={QuoteOrSpellNull(a?.Id)},"
                                    + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                    + $" OperationName={QuoteOrSpellNull(a?.OperationName)}.");

                    diagnosticSource.StopActivity(a, "Stop-Activity-Args-Object");
                    Console.WriteLine($"Stopped activity (\"{activityMoniker}\"):"
                                    + $" Id={QuoteOrSpellNull(a?.Id)},"
                                    + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                    + $" OperationName={QuoteOrSpellNull(a?.OperationName)}.");
                }
                catch (NullReferenceException nrRx)
                {
                    Console.WriteLine(nrRx.ToString());
                }

                a?.Dispose();
                Console.WriteLine($"Disposed activity (\"{activityMoniker}\"):"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)}.");
            }

            Console.WriteLine();
            ConfigureActivityListening();
            Console.WriteLine();

            {
                const string activityMoniker = "Activity 4.";
                Console.WriteLine($"About to create activity \"{activityMoniker}\" (using ActivitySource).");

                Activity a = activitySource.StartActivity(activityMoniker, ActivityKind.Server);
                Console.WriteLine($"Created activity (\"{activityMoniker}\"):"
                                + $" Activity is non-null: {a != null},"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");

                a?.Dispose();
                Console.WriteLine($"Stopped activity (\"{activityMoniker}\"):"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");
            }

            Console.WriteLine();

            {
                const string activityMoniker = "Activity 5";
                Console.WriteLine($"About to create activity \"{activityMoniker}\" (using A. ctor, Start(), Stop()).");

                Activity a = new Activity(activityMoniker);
                Console.WriteLine($"Created activity (\"{activityMoniker}\"):"
                                + $" Activity is non-null: {a != null},"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");
                a.Start();
                Console.WriteLine($"Started activity (\"{activityMoniker}\"):"
                                + $" Activity is non-null: {a != null},"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");

                a?.Stop();
                Console.WriteLine($"Stopped activity (\"{activityMoniker}\"):"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");

                a?.Dispose();
                Console.WriteLine($"Disposed activity (\"{activityMoniker}\"):"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)},"
                                + $" StartTimeUtc={QuoteOrSpellNull(a?.StartTimeUtc.ToString())}.");
            }

            Console.WriteLine();

            {
                const string activityMoniker = "Activity 6";
                Console.WriteLine($"About to create activity \"{activityMoniker}\" (using A.ctor, DS.StartA(..), DS.StopA(..).");

                Activity a = new Activity(activityMoniker);
                Console.WriteLine($"Created activity (\"{activityMoniker}\"):"
                                + $" Activity is non-null: {a != null},"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)}.");

                diagnosticSource.StartActivity(a, "Start-Activity-Args-Object");
                Console.WriteLine($"Started activity (\"{activityMoniker}\"):"
                                + $" Activity is non-null: {a != null},"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)}.");

                diagnosticSource.StopActivity(a, "Stop-Activity-Args-Object");
                Console.WriteLine($"Stopped activity (\"{activityMoniker}\"):"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)}.");

                a?.Dispose();
                Console.WriteLine($"Disposed activity (\"{activityMoniker}\"):"
                                + $" Id={QuoteOrSpellNull(a?.Id)},"
                                + $" DisplayName={QuoteOrSpellNull(a?.DisplayName)},"
                                + $" OperationName={QuoteOrSpellNull(a?.OperationName)}.");
            }

            Console.WriteLine();

            {
                const string activityMoniker = "Non-Activity 7";
                Console.WriteLine($"About to create activity \"{activityMoniker}\" (using DiagnosticSource.Write(..), no Activities.");

                Console.WriteLine($"About to write Start event (\"{activityMoniker}\").");
                diagnosticSource.Write("Custom-Event-Activity-Starting", "Custom-Start-Activity-Args-Object");
                Console.WriteLine($"Written Start event (\"{activityMoniker}\").");

                Console.WriteLine($"About to write End event (\"{activityMoniker}\").");
                diagnosticSource.Write("Custom-Event-Activity-Ending", "Custom-Start-Activity-Args-Object");
                Console.WriteLine($"Written End event (\"{activityMoniker}\").");
            }

            Console.WriteLine();
            Console.WriteLine("Finished. Press Enter.");
            Console.ReadLine();
        }


        private class SpecialActivity : Activity
        {
            public SpecialActivity(string operationName)
                : base(operationName)
            {

            }
        }

        private void ConfigureActivityListening()
        {
            Action<Activity, string> activityStartedHandler = (Activity a, string actSrcFilter) =>
                    Console.WriteLine($"EVENT(ActivityListener) 'ActivityStarted':"
                                    + $" actSrcFilter={QuoteOrSpellNull(actSrcFilter)}"
                                    + $" DisplayName={QuoteOrSpellNull(a.DisplayName)},"
                                    + $" OperationName={QuoteOrSpellNull(a.OperationName)},"
                                    + $" Id={QuoteOrSpellNull(a.Id)}.");

            Action<Activity, string> activityStoppedHandler = (Activity a, string actSrcFilter) =>
                    Console.WriteLine($"EVENT(ActivityListener) 'ActivityStopped':"
                                    + $" actSrcFilter={QuoteOrSpellNull(actSrcFilter)}"
                                    + $" DisplayName={QuoteOrSpellNull(a.DisplayName)},"
                                    + $" OperationName={QuoteOrSpellNull(a.OperationName)},"
                                    + $" Id={QuoteOrSpellNull(a.Id)}.");

            Action<ActivitySource, string, bool> shouldListenToHandlerPartial = (ActivitySource src, string actSrcFilter, bool result) =>
                    Console.WriteLine($"EVENT(ActivityListener) 'ShouldListenTo':"
                                    + $" actSrcFilter={QuoteOrSpellNull(actSrcFilter)}"
                                    + $" Name={QuoteOrSpellNull(src.Name)},"
                                    + $" Version={QuoteOrSpellNull(src.Version)},"
                                    + $" result={result}.");

            Func<ActivityCreationOptions<string>, string, ActivitySamplingResult> sampleUsingParentIdHandler = (ActivityCreationOptions<string> opts, string actSrcFilter) => {
                Console.WriteLine($"EVENT(ActivityListener) 'SampleUsingParentId':"
                                + $" actSrcFilter={QuoteOrSpellNull(actSrcFilter)}"
                                + $" Name={QuoteOrSpellNull(opts.Name)},"
                                + $" Kind={QuoteOrSpellNull(opts.Kind.ToString())},"
                                + $" Source.Name={QuoteOrSpellNull(opts.Source.Name)}");
                return ActivitySamplingResult.AllDataAndRecorded;
            };

            Func<ActivityCreationOptions<ActivityContext>, string, ActivitySamplingResult> sampleHandler = (ActivityCreationOptions<ActivityContext> opts, string actSrcFilter) => {
                Console.WriteLine($"EVENT(ActivityListener) 'Sample':"
                                + $" actSrcFilter={QuoteOrSpellNull(actSrcFilter)}"
                                + $" Name={QuoteOrSpellNull(opts.Name)},"
                                + $" Kind={QuoteOrSpellNull(opts.Kind.ToString())},"
                                + $" Source.Name={QuoteOrSpellNull(opts.Source.Name)}");
                return ActivitySamplingResult.AllDataAndRecorded;
            };

            Console.WriteLine($"Creating ActivityListener ({TestActivitySourceName})");
            ActivityListener targetedListener = new ActivityListener()
            {
                ActivityStarted = (Activity a) => activityStartedHandler(a, TestActivitySourceName),

                ActivityStopped = (Activity a) => activityStoppedHandler(a, TestActivitySourceName),

                ShouldListenTo = (ActivitySource src) => {
                    bool res = src.Name.Equals(TestActivitySourceName);
                    shouldListenToHandlerPartial(src, TestActivitySourceName, res);
                    return res;
                },

                SampleUsingParentId = (ref ActivityCreationOptions<string> opts) => sampleUsingParentIdHandler(opts, TestActivitySourceName),

                Sample = (ref ActivityCreationOptions<ActivityContext> opts) => sampleHandler(opts, TestActivitySourceName),
            };

            Console.WriteLine($"Installing ActivityListener ({TestActivitySourceName})");
            ActivitySource.AddActivityListener(targetedListener);

            Console.WriteLine($"Finished setting up ActivityListener ({TestActivitySourceName}).");

            const string EmptyNameListenerMoniker = "Empty-Name";

            Console.WriteLine($"Creating ActivityListener ({EmptyNameListenerMoniker})");
            ActivityListener catchAllListener = new ActivityListener()
            {
                ActivityStarted = (Activity a) => activityStartedHandler(a, EmptyNameListenerMoniker),

                ActivityStopped = (Activity a) => activityStoppedHandler(a, EmptyNameListenerMoniker),

                ShouldListenTo = (ActivitySource src) => {
                    bool res = src.Name.Equals(String.Empty);
                    shouldListenToHandlerPartial(src, EmptyNameListenerMoniker, res);
                    return res;
                },

                SampleUsingParentId = (ref ActivityCreationOptions<string> opts) => sampleUsingParentIdHandler(opts, EmptyNameListenerMoniker),

                Sample = (ref ActivityCreationOptions<ActivityContext> opts) => sampleHandler(opts, EmptyNameListenerMoniker),
            };

            Console.WriteLine($"Installing ActivityListener ({EmptyNameListenerMoniker})");
            ActivitySource.AddActivityListener(catchAllListener);

            Console.WriteLine($"Finished setting up ActivityListener ({EmptyNameListenerMoniker}).");
        }

        private void ConfigureDiagnosticListening()
        {
            Console.WriteLine($"Creating DiagnosticListener");

            DiagnosticListener.AllListeners.Subscribe(
                    new ConfigurableObserver<DiagnosticListener>("DiagnosticListener-SUBSCRIPTION-Listener")
                    {
                        OnNextHandler = (dl) => {
                            Console.WriteLine($"EVENT(DiagnosticListener-Subscription) 'OnNext': dl.Name={QuoteOrSpellNull(dl.Name)}");
                            dl.Subscribe(new ConfigurableObserver<KeyValuePair<string, object>>("DiagnosticListener-EVENT-Listener")
                            {
                                OnNextHandler = (ev) =>
                                {
                                    Console.WriteLine($"EVENT(DiagnosticListener-Subscription) 'OnNext': ev.Key={QuoteOrSpellNull(ev.Key)}"
                                                    + $" ev.Value.GetType()={QuoteOrSpellNull(ev.Value?.GetType()?.FullName)}"
                                                    + $" ev.Value={QuoteOrSpellNull(ev.Value?.ToString())}");
                                }
                            });
                        }
                    }
            );

            Console.WriteLine($"Finished setting up DiagnosticListener.");
        }

        private class ConfigurableObserver<T> : IObserver<T>
        {
            private readonly string _observerName;

            public ConfigurableObserver(string observerName)
            {
                ValidateNotNull(observerName, nameof(observerName));
                _observerName = observerName;
            }

            public Action OnCompletedHandler { get; set; } = null;
            public Action<Exception> OnErrorHandler { get; set; } = null;
            public Action<T> OnNextHandler { get; set; } = null;


            void IObserver<T>.OnCompleted() { if (OnCompletedHandler != null) OnCompletedHandler.Invoke(); else Console.WriteLine($"ConfigurableObserver(\"{_observerName}\").OnCompleted-Null-Invoked)"); }
            void IObserver<T>.OnError(Exception error) { if (OnErrorHandler != null) OnErrorHandler.Invoke(error); else Console.WriteLine($"ConfigurableObserver(\"{_observerName}\").OnError-Null-Invoked)"); }
            void IObserver<T>.OnNext(T value) { if (OnNextHandler != null) OnNextHandler.Invoke(value); else Console.WriteLine($"ConfigurableObserver(\"{_observerName}\").OnNext-Null-Invoked)"); }
        }

        public static void ValidateNotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name ?? "parameter");
            }
        }

        public static string QuoteOrSpellNull(string str)
        {
            if (str == null)
            {
                return "<null>";
            }

            var builder = new StringBuilder();
            builder.Append('"');
            builder.Append(str);
            builder.Append('"');

            return builder.ToString();
        }
    }
}
