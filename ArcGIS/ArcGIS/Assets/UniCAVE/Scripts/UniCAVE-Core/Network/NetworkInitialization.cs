// MIT License
// ...
// rest of the license header

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k; // Ensure you add this using directive
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniCAVE
{
    /// <summary>
    /// Starts the program as either client or server depending on machine
    /// </summary>
    public class NetworkInitialization : MonoBehaviour
    {
        public static int TimeoutWaitTime = 20; //make this changeable from command line?

        public NetworkManager networkManager;

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("headMachine")]
        string oldHeadMachine = "C6_V1_HEAD";

        [SerializeField]
        MachineName headMachineAsset;

        public string headMachine => MachineName.GetMachineName(oldHeadMachine, headMachineAsset);

        [Tooltip("This can be overridden at runtime with parameter serverAddress, for example \"serverAddress 192.168.0.100\"")]
        public string serverAddress = "192.168.4.140";

        [Tooltip("This can be overridden at runtime with parameter serverPort, for example \"serverPort 8421\"")]
        public int serverPort = 7568;

        /// <summary>
        /// Starts as client or server.
        /// </summary>
        void Start()
        {
            string serverArg = Util.GetArg("serverAddress");
            if (serverArg != null)
            {
                serverAddress = serverArg;
            }

            string portArg = Util.GetArg("serverPort");
            if (portArg != null)
            {
                int.TryParse(portArg, out serverPort);
            }

            string runningMachineName = Util.GetMachineName();
            Debug.Log($"serverAddress = {serverAddress}, serverPort = {serverPort}, headMachine = {headMachine}, runningMachine = {runningMachineName}");

            networkManager.networkAddress = serverAddress;

            // Set the port in the transport component
            var transport = networkManager.GetComponent<Transport>();
            if (transport is KcpTransport kcpTransport)
            {
                kcpTransport.Port = (ushort)serverPort;
            }

#if !UNITY_EDITOR
            if ((Util.GetArg("forceClient") == "1") || (Util.GetMachineName() != headMachine))
            {
                networkManager.StartClient();
            }
            else
            {
                networkManager.StartServer();
            }
#else
            networkManager.StartServer();
#endif
        }

        /// <summary>
        /// Quits after 20 seconds if no connection is made to server.
        /// </summary>
        void Update()
        {
            if (Util.GetMachineName() != oldHeadMachine)
            {
                if (!NetworkClient.isConnected && !NetworkServer.active)
                {
                    networkManager.StartClient();
                }

                if (Time.time > TimeoutWaitTime && !NetworkClient.isConnected)
                {
                    Application.Quit();
                }
            }
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(NetworkInitialization))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(networkManager)));

                // Special handling for old machine names:
                SerializedProperty oldMachineName = serializedObject.FindProperty(nameof(oldHeadMachine));
                SerializedProperty machineName = serializedObject.FindProperty(nameof(headMachineAsset));
                MachineName.DrawDeprecatedMachineName(oldMachineName, machineName, "HeadMachine");

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(serverAddress)));

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(serverPort)));

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
