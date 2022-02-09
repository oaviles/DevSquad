using System;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using System.Security.Cryptography;

namespace ThermostatSimulator
{
    class Program
    {
        // Update the variables to use your deployed services
        static string idScope = "Your-ID-SCOPE";
        static string enrollmentKey = "Your-PrimaryKey-of-DPS-EnrolllmentGroup";

        // Telemetry globals.
        const int intervalInMilliseconds = 10000;        // Time interval required by wait function.
        static string deviceID = "ThermostatSimulator1";
        static int sensorNum = 1;
        static string sensorIdentification = "Sensor number " + sensorNum;
        static double tempContents = 20;                // Current temperature of contents, in degrees C.
        static double humidityContents = 30;            // Humidity
        static double interval = 60;                    // Simulated time interval, in seconds.
        static double timeOnCurrentTask = 0;            // Time on current task, in seconds.
        static Random rand;

        // IoT  global variables.
        static DeviceClient s_deviceClient;
        static CancellationTokenSource cts;
        static string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
        static TwinCollection reportedProperties = new TwinCollection();



        static double Degrees2Radians(double deg)
        {
            return deg * Math.PI / 180;
        }


        static void UpdateSensor()
        {
            tempContents = new Random().Next(20, 30);
            humidityContents  = new Random().Next(30, 50);
            timeOnCurrentTask += interval;
        }

        static async void SendSensorTelemetryAsync(Random rand, CancellationToken token)
        {
            while (true)
            {
                UpdateSensor();

                // Create the telemetry JSON message.
                var telemetryDataPoint = new
                {
                    Temperature = Math.Round(tempContents, 2)
                    ,Humidity = Math.Round(humidityContents, 2)
                };
                var telemetryMessageString = JsonSerializer.Serialize(telemetryDataPoint);
                var telemetryMessage = new Message(Encoding.ASCII.GetBytes(telemetryMessageString));

                Console.Write($"Telemetry data: {telemetryMessageString} | ");

                // Bail if requested.
                token.ThrowIfCancellationRequested();

                // Send the telemetry message.
                await s_deviceClient.SendEventAsync(telemetryMessage);
                Console.WriteLine($"Telemetry sent {DateTime.Now.ToString()}");

                await Task.Delay(intervalInMilliseconds);
            }
        }

        static void Main(string[] args)
        {

            rand = new Random();
            Console.WriteLine($"Starting {sensorIdentification}", ConsoleColor.Yellow);

            try
            {
                var primaryKey = ComputeDerivedSymmetricKey(enrollmentKey, deviceID);

                using (var security = new SecurityProviderSymmetricKey(deviceID, primaryKey, null))
                {
                    DeviceRegistrationResult result = RegisterDeviceAsyncDPS().GetAwaiter().GetResult();
                    if (result.Status != ProvisioningRegistrationStatusType.Assigned)
                    {
                        Console.WriteLine("Failed to register device");
                        return;
                    }
                    IAuthenticationMethod auth = new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId, (security as SecurityProviderSymmetricKey).GetPrimaryKey());
                    s_deviceClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt);
                }
                Console.WriteLine("Device successfully connected to Azure IoT Hub");

                Console.Write("Register settings changed handler...");
                Console.WriteLine("Done");
                Console.WriteLine("\n\nPress any key to exit...\n\n");

                cts = new CancellationTokenSource();

                SendSensorTelemetryAsync(rand, cts.Token);
                
                Console.ReadKey();
                cts.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task<DeviceRegistrationResult> RegisterDeviceAsyncDPS()
        {
            Console.WriteLine("Register device...");

            var primaryKey = ComputeDerivedSymmetricKey(enrollmentKey, deviceID);
            // Create symmetric key with the generated primary key
            using (var security = new SecurityProviderSymmetricKey(deviceID, primaryKey, null))
            using (var transportHandler = new ProvisioningTransportHandlerMqtt())
            {
                // Create a Provisioning Device Client
                var client = ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, idScope, security, transportHandler);

                // Register the device using the symmetric key and MQTT
                DeviceRegistrationResult result = await client.RegisterAsync();

                return result;
            }
            
        }

        /// <summary>
        /// Compute a symmetric key for the provisioned device from the enrollment group symmetric key used in attestation.
        /// </summary>
        /// <param name="enrollmentKey">Enrollment group symmetric key.</param>
        /// <param name="deviceId">The device Id of the key to create.</param>
        /// <returns>The key for the specified device Id registration in the enrollment group.</returns>
        /// <seealso>
        /// https://docs.microsoft.com/en-us/azure/iot-edge/how-to-auto-provision-symmetric-keys?view=iotedge-2018-06#derive-a-device-key
        /// </seealso>
        private static string ComputeDerivedSymmetricKey(string enrollmentKey, string deviceId)
        {
            if (string.IsNullOrWhiteSpace(enrollmentKey))
            {
                return enrollmentKey;
            }

            var key = "";
            using (var hmac = new HMACSHA256(Convert.FromBase64String(enrollmentKey)))
            {
                key = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId)));
            }

            return key;
        }

    }

}