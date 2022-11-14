using EPiServer.ServiceLocation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(IReferenceCounter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ReferenceCounter : IReferenceCounter
    {
        IDictionary<object,int> dictionary = new ConcurrentDictionary<object, int>();

        public void AddReference(object src)
        {
            try
            {
                int value;
                if (dictionary.TryGetValue(src, out value))
                {   // inc the count
                    dictionary.Remove(src);
                    dictionary.Add(src, ++value);
                }
                else
                {   // make the count 1
                    dictionary.Add(src, 1);
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void RemoveReference(object src)
        {
            int value;
            if (dictionary.TryGetValue(src, out value))
            {
                if (value > 1)
                {   // decrement the curent count
                    dictionary.Remove(src);
                    dictionary.Add(src, --value);
                } 
                else
                {   // remove the ref from the collection
                    dictionary.Remove(src);
                }
            }
        }
        public bool hasReference(object src)
        {
            // if its in the dictionary, we have a reference.
            return dictionary.ContainsKey(src);
        }

        public int getReferenceCount(object src)
        {
            int value;
            dictionary.TryGetValue(src, out value);
            return value;                
        }
    }
}
