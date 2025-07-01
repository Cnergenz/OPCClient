using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Opc.Ua;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPCCllientExample
{
    public class OPC
    {
        private Subscription subscription = new Subscription() 
        { 
            PublishingInterval  = 1000,
            PublishingEnabled = true,
        };
        private object sessionlock = new object();
        private Session session;
        private bool _isConnected = false;
        private ApplicationInstance applicationInstance = new ApplicationInstance();
        private string filePath = "OPCSetting.xml";

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    if (!value)
                    {
                        if (session != null)
                        {
                            session.Close();
                            session.Dispose();
                        }
                        session = null;
                    }
                    _isConnected = value;
                }
            }
        }

        private async Task<bool> LoadConfig()
        {
            try
            {
                // Load application configuration asynchronously
                await applicationInstance.LoadApplicationConfiguration(filePath, silent: false);
                await applicationInstance.CheckApplicationInstanceCertificate(false, 0);
                Console.WriteLine("LoadConfig success");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task<EndpointDescription> SelectEndpointAsync(string endpointUrl, bool useSecurity)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return CoreClientUtils.SelectEndpoint(endpointUrl, useSecurity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Slectendpoint");
                    Console.WriteLine(ex.Message);
                    throw;
                }
            });
        }

        public async Task<bool> ConnectOPC(string ip, string port)
        {
            try
            {
                await LoadConfig();
                
                var endpointUrl = $"opc.tcp://{ip}:{port}";
                var selectedEndpoint = await SelectEndpointAsync(endpointUrl, useSecurity: false);
                //var userIdentity = new UserIdentity("Owner", "");
                //session = await Session.Create(
                //   applicationInstance.ApplicationConfiguration,
                //   new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(applicationInstance.ApplicationConfiguration)),
                //   false, "", 10000, userIdentity, null);
                session = await Session.Create(
                   applicationInstance.ApplicationConfiguration,
                   new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(applicationInstance.ApplicationConfiguration)),
                   false, "", 10000, null, null);
                if (session != null && session.Connected)
                {
                    IsConnected = true;
                    Console.WriteLine("Connect Successfully");

                }
                else
                {
                    IsConnected = false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                IsConnected = false;
            }
            return IsConnected;
        }

        public void DisconnectOPC()
        {
            try
            {
                if (IsConnected)
                {
                    IsConnected = false;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public int ReadMode(string nodeId)
        {
            try
            {
                lock (sessionlock)
                {
                    var result = session.ReadValue(new NodeId(nodeId));
                    if (result.Value is Int16 value)
                    {
                        return Convert.ToInt32(value);
                    }
                    //else if (result.Value is Int32 val)
                    //{
                    //    return val;
                    //}
                    //else if (result.Value is Int64 val64)
                    //{
                    //    return Convert.ToInt32(val64);
                    //}
                    else
                    {
                        throw new Exception();
                    }
                }

            }
            catch (Exception ex)
            {
                IsConnected = false;
                throw;
            }

        }

        public double ReadDouble(string nodeId)
        {
            try
            {
                lock (sessionlock)
                {
                    var result = session.ReadValue(new NodeId(nodeId));
                    if (result.Value is double value)
                    {
                        return Convert.ToDouble(value);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

            }
            catch (Exception ex)
            {
                IsConnected = false;
                throw;
            }

        }

        public bool ReadNode(string nodeId)
        {
            try
            {
                lock (sessionlock)
                {
                    var result = session.ReadValue(new NodeId(nodeId));
                    if (result.Value is bool value)
                    {
                        return value;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

            }
            catch (Exception ex)
            {
                IsConnected = false;
                throw;
            }

        }

        public int ReadTime(string nodeId)
        {
            try
            {
                lock (sessionlock)
                {
                    var result = session.ReadValue(new NodeId(nodeId));
                    if (result.Value is Int64 value)
                    {
                        return Convert.ToInt32(value);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

            }
            catch (Exception ex)
            {
                IsConnected = false;
                throw;
            }

        }

        public void WriteNode(string nodeId, object data)
        {
            try
            {
                lock (sessionlock)
                {
                    var node = new NodeId(nodeId);
                    var nodeAttributes = session.ReadValue(node);
                    var expectedType = nodeAttributes.WrappedValue.TypeInfo.BuiltInType;
                    object valueToWrite = data;
                    switch (expectedType)
                    {
                        case BuiltInType.Int32:
                            valueToWrite = Convert.ToInt16(data);
                            break;
                        case BuiltInType.String:
                            valueToWrite = data.ToString();
                            break;
                        case BuiltInType.Double:
                            valueToWrite = Convert.ToDouble(data);
                            break;
                        case BuiltInType.Int16:
                            valueToWrite = Convert.ToInt16(data);
                            break;
                        // Add more cases for different types
                        case BuiltInType.Boolean:
                            valueToWrite = Convert.ToBoolean(data);
                            break;
                    }
                    var writeValue = new DataValue
                    {
                        Value = new Variant(valueToWrite),
                        StatusCode = StatusCodes.Good,
                        SourceTimestamp = DateTime.UtcNow
                    };

                    StatusCodeCollection results = null;
                    DiagnosticInfoCollection diagnosticInfos = null;
                    try
                    {
                        session.Write(null, new WriteValueCollection { new WriteValue { NodeId = node, AttributeId = Attributes.Value, Value = writeValue } }, out results, out diagnosticInfos);
                    }
                    catch
                    {
                        throw new Exception("Write Fail");
                    }
                    if (results != null && results[0] == StatusCodes.Good)
                    {
                        return;
                    }
                    else
                    {
                        throw new Exception();
                    }


                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("The error in write is :" + ex.ToString());
                IsConnected = false;
                throw;
            }
        }

       

        public void AddMonitoredItem(NodeId nodeId, string displayName)
        {
            MonitoredItem monitoredItem = new MonitoredItem()
            {
                StartNodeId = nodeId,
                AttributeId = Attributes.Value,
                DisplayName = displayName,
                SamplingInterval = 1000,
                QueueSize = 1,
                DiscardOldest = true
            };
            monitoredItem.Notification += (monItem, args) =>
            {
                Console.WriteLine("Hello world");
                var val = monItem.DequeueValues().FirstOrDefault();
                if (val != null)
                {
                    Console.WriteLine($"{monItem.StartNodeId} + {val.Value}");
                }
            };
            subscription.AddItem(monitoredItem);
        }

        public void Subscribe()
        {
            session.AddSubscription(subscription);
            subscription.Create();
        }
    }
}
