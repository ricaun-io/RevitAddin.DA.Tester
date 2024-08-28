using DesignAutomationFramework;
using System;
using System.Linq;

namespace RevitAddin.DA.Tester.Revit
{
    public class DesignAutomation<T> : DesignAutomation where T : IDesignAutomation
    {
        public DesignAutomation() : base(Activator.CreateInstance(typeof(T)))
        {

        }
        public DesignAutomation(T instance) : base(instance)
        {

        }
    }

    public class DesignAutomation : IDisposable
    {
        private readonly object instance;

        public DesignAutomation(Type type)
        {
            this.instance = Activator.CreateInstance(type);
            Initialize();
        }

        public DesignAutomation(object instance)
        {
            this.instance = instance;
            Initialize();
        }

        public virtual void Initialize()
        {
            Console.WriteLine($"{nameof(DesignAutomation)} Initialize: \t{instance}");
            DesignAutomationBridge.DesignAutomationReadyEvent += DesignAutomationReadyEvent;
        }

        public void Dispose()
        {
            Console.WriteLine($"{nameof(DesignAutomation)} Dispose: \t{instance}");
            DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationReadyEvent;
        }

        private void DesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationReadyEvent;

            var data = e.DesignAutomationData;

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"RevitApp: {data.RevitApp} FilePath: {data.FilePath} RevitDoc: {data.RevitDoc}");
            Console.WriteLine("--------------------------------------------------");

            try
            {
                var method = instance.GetType().GetMethods()
                    .Where(e => e.Name.Equals(nameof(IDesignAutomation.Execute)))
                    .FirstOrDefault(e => e.GetParameters().Count() == 3);

                Console.WriteLine($"Invoke: {method}");

                var result = method.Invoke(instance, new object[] { data.RevitApp, data.FilePath, data.RevitDoc });

                if (result is bool resultBool)
                    e.Succeeded = resultBool;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(DesignAutomation)} Invoke Exception: \t{ex.Message}");
                throw;
            }
        }
    }
}