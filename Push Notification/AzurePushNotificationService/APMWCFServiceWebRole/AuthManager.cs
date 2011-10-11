// --------------------------------------------------------------------
// <copyright file="AuthManager.xaml.cs" company="Microsoft Corp">
// Copyright 2010 Microsoft Corp
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// ---------------------------------------------------------------------

namespace ApmWcfServiceWebRole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Web;
    using Microsoft.ServiceModel.Web;
    using MsgHelperLib.Common;
    using MsgHelperLib.Helpers;
    using MsgHelperLib.Messages;
    using MsgHelperLib.Model;
    using MsgHelperLib.Queue;

    /// <summary>
    /// This class handles HTTP basic auth since WCF does not handle HTTP basic by itself.
    /// </summary>
    internal static class AuthManager
    {
        private const string BasicAuth = "Basic";

        /// <summary>
        /// Creds are currently managed internally. But these could move to azure d/b.
        /// If you do not want to save the password in clear in the d/b or in code, do a oneway hash, store and check.
        /// </summary>
        private static Cred[] creds =
            new Cred[] 
            { 
                new Cred { Uname = "tony", Pwd = "clifton" }, 
                new Cred { Uname = "bill", Pwd = "clinton" }, 
                new Cred { Uname = "condi", Pwd = "rice" } 
            };

        /// <summary>
        /// First decode the credentials and authenticate the creds if pwd matches the usernmame.
        /// If the credentials are missing, we send 401 back.
        /// </summary>
        /// <returns>True if the credentials are correct. False otherwise.</returns>
        public static bool AuthenticateUser()
        {
            // Credentials are Base64 encoded in Basic. First get the encoded string
            string encodedCreds = GetEncodedCredentialsFromHeader();
            if (!String.IsNullOrEmpty(encodedCreds))
            {
                // Decode the credntials and check if they are valid
                byte[] decodedBytes = null;
                try
                {
                    decodedBytes = Convert.FromBase64String(encodedCreds);
                }
                catch (FormatException)
                {
                    return false;
                }

                string credentials = ASCIIEncoding.ASCII.GetString(decodedBytes);
                return IsValidCredential(credentials);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Constructs an auth response header for the HTTP response. 
        /// </summary>
        public static void ConstructAuthResponse()
        {
            const string RealmFormatString = "Basic realm=\"{0}\"";
            const string AuthServerHeader = "WWW-Authenticate";
            const string AccessDeniedStatus = "Access Denied";

            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
            WebOperationContext.Current.OutgoingResponse.StatusDescription = AccessDeniedStatus;

            // return a challenge response  
            Uri uri = WebOperationContext.Current.IncomingRequest.GetRequestUri();
            WebOperationContext.Current.OutgoingResponse.Headers.Add(AuthServerHeader, string.Format(CultureInfo.InvariantCulture, RealmFormatString, uri.GetLeftPart(UriPartial.Authority)));
        }

        /// <summary>
        /// Checks user name password for authentication.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static bool CheckUserNamePwd(string username, string password)
        {
            // iterate through all creds to find our user
            // your code logic here to authenticate user
            foreach (Cred c in creds)
            {
                if (username == c.Uname && password == c.Pwd)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Basic auth encodes uname and pwd pair. We take the credential string from the HTTP header.
        /// </summary>
        /// <returns></returns>
        private static string GetEncodedCredentialsFromHeader()
        {
            WebOperationContext ctx = WebOperationContext.Current;

            // credentials are in the Authorization Header
            string credsHeader = ctx.IncomingRequest.Headers[HttpRequestHeader.Authorization];
            if (credsHeader != null)
            {
                // make sure that we have 'Basic' auth header. Anything else can't be handled
                string creds = null;
                int credsPosition = credsHeader.IndexOf(BasicAuth, StringComparison.OrdinalIgnoreCase);
                if (credsPosition != -1)
                {
                    // 'Basic' creds were found
                    credsPosition += BasicAuth.Length + 1;
                    if (credsPosition < credsHeader.Length - 1)
                    {
                        creds = credsHeader.Substring(credsPosition, credsHeader.Length - credsPosition);
                        return creds;
                    }
                    return null;
                }
                else
                {
                    // we did not find Basic auth header but some other type of auth. We can't handle it. Return null.
                    return null;
                }
            }

            // no auth header was found
            return null;
        }

        /// <summary>
        /// This routine, we split the user name:pwd string and ask CheckUserNamePWd to make sure that credentials are valid.
        /// </summary>
        /// <param name="creds">Creds provide the user credential string.</param>
        /// <returns>Returns true if creds are valid.</returns>
        private static bool IsValidCredential(string creds)
        {
            string[] authParts = creds.Split(':');
            if (authParts.Length == 2)
            {
                string userid = authParts[0];
                string password = authParts[1];
                if (CheckUserNamePwd(userid, password))
                {
                    return true;
                }
            }

            return false;
        }

        private struct Cred
        {
            public string Uname;
            public string Pwd;
        }
    }
}
