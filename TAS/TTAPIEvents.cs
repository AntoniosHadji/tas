﻿using System;
using System.Diagnostics;

namespace TAS
{

    using TradingTechnologies.TTAPI;

    public partial class TTAPIEvents : IDisposable
    {
        /// <summary>
        /// Declare the TTAPI objects
        /// </summary>
        private UniversalLoginTTAPI apiInstance = null;
        private WorkerDispatcher workDispatch = null;
        private bool disposed = false;
        private object m_lock = new object();
        private string username = string.Empty;
        private string password = string.Empty;
        private string ttConfig = "orders.ini";

        private bool ready = true;

        #region Constructors
        private TTAPIEvents()
        {
        }
        public TTAPIEvents(string u, string p)
        {
            Console.WriteLine("TTAPI starting for user: {0}", u);
            username = u;
            password = p;

        }

        public TTAPIEvents(string u, string p, string c)
          
        {
            username = u;
            password = p;
            ttConfig = c;

        } 
        #endregion
        
        /// <summary>
        /// Create and start the Dispatcher
        /// </summary>
        public void Start()
        {
            // Attach a WorkerDispatcher to the current thread
            workDispatch = Dispatcher.AttachWorkerDispatcher();
            workDispatch.BeginInvoke(new Action(Init));
            workDispatch.Run();
        }

        /// <summary>
        /// Initialize TT API
        /// </summary>
        public void Init()
        {
            // Use "Universal Login" Login Mode
            TTAPI.UniversalLoginModeDelegate ulDelegate = new
            TTAPI.UniversalLoginModeDelegate(ttApiInitComplete);
            TTAPI.CreateUniversalLoginTTAPI(Dispatcher.Current, ulDelegate);
        }

        /// <summary>
        /// Event notification for status of TT API initialization
        /// </summary>
        public void ttApiInitComplete(UniversalLoginTTAPI api, Exception ex)
        {
            if (ex == null)
            {
                Console.WriteLine("TT API Initialization Succeeded");
                // Authenticate your credentials
                apiInstance = api;
                apiInstance.AuthenticationStatusUpdate += new
                EventHandler<AuthenticationStatusUpdateEventArgs>(
                apiInstance_AuthenticationStatusUpdate);
                apiInstance.Authenticate(username, password);
            }
            else
            {
                Console.WriteLine("TT API Initialization Failed: {0}", ex.Message);
                Dispose();
            }
        }

        /// <summary>
        /// Event notification for status of authentication
        /// </summary>
        public void apiInstance_AuthenticationStatusUpdate(object sender,
            AuthenticationStatusUpdateEventArgs e)
        {
            if (e.Status.IsSuccess)
            {
                Console.WriteLine("TT API User Authentication Succeeded: {0}", e.Status.StatusMessage);
                
                // Add code here to begin working with the TT API
                readParameters(ttConfig);
            }
            else
            {
                Console.WriteLine("TT API User Authentication failed: {0}", e.Status.StatusMessage);
                Dispose();
            }
        }

        /// <summary>
        /// Shuts down the TT API
        /// </summary>
        public void Dispose()
        {
            Debug.WriteLine("TTAPIFunctions::Dispose method called");
            lock (m_lock)
            {
                if (!disposed)
                {
                    // Shutdown the TT API
                    if (apiInstance != null)
                    {
                        apiInstance.Shutdown();
                        apiInstance = null;
                    }
                    // Shutdown the Dispatcher
                    if (workDispatch != null)
                    {
                        workDispatch.BeginInvokeShutdown();
                        workDispatch = null;
                    }
                    disposed = true;
                }
            }
        }

        }
    }
