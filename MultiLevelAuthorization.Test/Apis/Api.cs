
namespace MultiLevelAuthorization.Test.Apis
{
    using System = global::System;

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class AppController
    {
        private System.Net.Http.HttpClient _httpClient;
        private System.Lazy<System.Text.Json.JsonSerializerOptions> _settings;

        public AppController(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
            _settings = new System.Lazy<System.Text.Json.JsonSerializerOptions>(CreateSerializerSettings);
        }

        private System.Text.Json.JsonSerializerOptions CreateSerializerSettings()
        {
            var settings = new System.Text.Json.JsonSerializerOptions();
            UpdateJsonSerializerSettings(settings);
            return settings;
        }

        protected System.Text.Json.JsonSerializerOptions JsonSerializerSettings { get { return _settings.Value; } }

        partial void UpdateJsonSerializerSettings(System.Text.Json.JsonSerializerOptions settings);

        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url);
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<string> AppsAsync(AppCreateRequest body = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps");

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    var content_ = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(body, _settings.Value));
                    content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    request_.Content = content_;
                    request_.Method = new System.Net.Http.HttpMethod("POST");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var result_ = (string)System.Convert.ChangeType(responseData_, typeof(string));
                            return result_;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<AppDto> InitAsync(string appId, AppInitRequest body = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (appId == null)
                throw new System.ArgumentNullException("appId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps/{appId}/init");
            urlBuilder_.Replace("{appId}", System.Uri.EscapeDataString(ConvertToString(appId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    var content_ = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(body, _settings.Value));
                    content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    request_.Content = content_;
                    request_.Method = new System.Net.Http.HttpMethod("POST");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<AppDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<string> AuthenticationTokenAsync(string appId, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (appId == null)
                throw new System.ArgumentNullException("appId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps/{appId}/authentication-token");
            urlBuilder_.Replace("{appId}", System.Uri.EscapeDataString(ConvertToString(appId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var result_ = (string)System.Convert.ChangeType(responseData_, typeof(string));
                            return result_;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            public T Object { get; }

            public string Text { get; }
        }

        public bool ReadResponseAsString { get; set; }

        protected virtual async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(System.Net.Http.HttpResponseMessage response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Threading.CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            if (ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = System.Text.Json.JsonSerializer.Deserialize<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (System.Text.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        var typedBody = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(responseStream, JsonSerializerSettings, cancellationToken).ConfigureAwait(false);
                        return new ObjectResponseResult<T>(typedBody, string.Empty);
                    }
                }
                catch (System.Text.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }

        private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is System.Enum)
            {
                var name = System.Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }

                    var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
            {
                return System.Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return System.Convert.ToBase64String((byte[])value);
            }
            else if (value.GetType().IsArray)
            {
                var array = System.Linq.Enumerable.OfType<object>((System.Array)value);
                return string.Join(",", System.Linq.Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }

            var result = System.Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class PermissionController
    {
        private System.Net.Http.HttpClient _httpClient;
        private System.Lazy<System.Text.Json.JsonSerializerOptions> _settings;

        public PermissionController(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
            _settings = new System.Lazy<System.Text.Json.JsonSerializerOptions>(CreateSerializerSettings);
        }

        private System.Text.Json.JsonSerializerOptions CreateSerializerSettings()
        {
            var settings = new System.Text.Json.JsonSerializerOptions();
            UpdateJsonSerializerSettings(settings);
            return settings;
        }

        protected System.Text.Json.JsonSerializerOptions JsonSerializerSettings { get { return _settings.Value; } }

        partial void UpdateJsonSerializerSettings(System.Text.Json.JsonSerializerOptions settings);

        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url);
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<PermissionGroupDto>> PermissionGroupsAsync(string appId, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (appId == null)
                throw new System.ArgumentNullException("appId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps/{appId}/permission-groups");
            urlBuilder_.Replace("{appId}", System.Uri.EscapeDataString(ConvertToString(appId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<System.Collections.Generic.ICollection<PermissionGroupDto>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            public T Object { get; }

            public string Text { get; }
        }

        public bool ReadResponseAsString { get; set; }

        protected virtual async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(System.Net.Http.HttpResponseMessage response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Threading.CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            if (ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = System.Text.Json.JsonSerializer.Deserialize<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (System.Text.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        var typedBody = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(responseStream, JsonSerializerSettings, cancellationToken).ConfigureAwait(false);
                        return new ObjectResponseResult<T>(typedBody, string.Empty);
                    }
                }
                catch (System.Text.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }

        private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is System.Enum)
            {
                var name = System.Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }

                    var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
            {
                return System.Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return System.Convert.ToBase64String((byte[])value);
            }
            else if (value.GetType().IsArray)
            {
                var array = System.Linq.Enumerable.OfType<object>((System.Array)value);
                return string.Join(",", System.Linq.Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }

            var result = System.Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class RoleController
    {
        private System.Net.Http.HttpClient _httpClient;
        private System.Lazy<System.Text.Json.JsonSerializerOptions> _settings;

        public RoleController(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
            _settings = new System.Lazy<System.Text.Json.JsonSerializerOptions>(CreateSerializerSettings);
        }

        private System.Text.Json.JsonSerializerOptions CreateSerializerSettings()
        {
            var settings = new System.Text.Json.JsonSerializerOptions();
            UpdateJsonSerializerSettings(settings);
            return settings;
        }

        protected System.Text.Json.JsonSerializerOptions JsonSerializerSettings { get { return _settings.Value; } }

        partial void UpdateJsonSerializerSettings(System.Text.Json.JsonSerializerOptions settings);

        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url);
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<PermissionGroupDto>> PermissionGroupsAsync(string appId, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (appId == null)
                throw new System.ArgumentNullException("appId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps/{appId}/role?");
            urlBuilder_.Replace("{appId}", System.Uri.EscapeDataString(ConvertToString(appId, System.Globalization.CultureInfo.InvariantCulture)));
            if (roleName != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("roleName") + "=").Append(System.Uri.EscapeDataString(ConvertToString(roleName, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (ownerId != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("ownerId") + "=").Append(System.Uri.EscapeDataString(ConvertToString(ownerId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (modifiedByUserId != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("modifiedByUserId") + "=").Append(System.Uri.EscapeDataString(ConvertToString(modifiedByUserId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Content = new System.Net.Http.StringContent(string.Empty, System.Text.Encoding.UTF8, "text/plain");
                    request_.Method = new System.Net.Http.HttpMethod("POST");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<Role>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<Role>> RolesAsync(string appId, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (appId == null)
                throw new System.ArgumentNullException("appId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps/{appId}/roles");
            urlBuilder_.Replace("{appId}", System.Uri.EscapeDataString(ConvertToString(appId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<System.Collections.Generic.ICollection<Role>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<RoleUser>> RoleUsersAsync(string appId, System.Guid? roleId = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (appId == null)
                throw new System.ArgumentNullException("appId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps/{appId}/role-users?");
            urlBuilder_.Replace("{appId}", System.Uri.EscapeDataString(ConvertToString(appId, System.Globalization.CultureInfo.InvariantCulture)));
            if (roleId != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("roleId") + "=").Append(System.Uri.EscapeDataString(ConvertToString(roleId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<System.Collections.Generic.ICollection<RoleUser>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            public T Object { get; }

            public string Text { get; }
        }

        public bool ReadResponseAsString { get; set; }

        protected virtual async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(System.Net.Http.HttpResponseMessage response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Threading.CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            if (ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = System.Text.Json.JsonSerializer.Deserialize<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (System.Text.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        var typedBody = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(responseStream, JsonSerializerSettings, cancellationToken).ConfigureAwait(false);
                        return new ObjectResponseResult<T>(typedBody, string.Empty);
                    }
                }
                catch (System.Text.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }

        private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is System.Enum)
            {
                var name = System.Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }

                    var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
            {
                return System.Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return System.Convert.ToBase64String((byte[])value);
            }
            else if (value.GetType().IsArray)
            {
                var array = System.Linq.Enumerable.OfType<object>((System.Array)value);
                return string.Join(",", System.Linq.Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }

            var result = System.Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class SecureObjectController
    {
        private System.Net.Http.HttpClient _httpClient;
        private System.Lazy<System.Text.Json.JsonSerializerOptions> _settings;

        public SecureObjectController(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
            _settings = new System.Lazy<System.Text.Json.JsonSerializerOptions>(CreateSerializerSettings);
        }

        private System.Text.Json.JsonSerializerOptions CreateSerializerSettings()
        {
            var settings = new System.Text.Json.JsonSerializerOptions();
            UpdateJsonSerializerSettings(settings);
            return settings;
        }

        protected System.Text.Json.JsonSerializerOptions JsonSerializerSettings { get { return _settings.Value; } }

        partial void UpdateJsonSerializerSettings(System.Text.Json.JsonSerializerOptions settings);

        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url);
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<Role> RoleAsync(string appId, string roleName = null, System.Guid? ownerId = null, System.Guid? modifiedByUserId = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (appId == null)
                throw new System.ArgumentNullException("appId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps/{appId}/secureObjectTypes");
            urlBuilder_.Replace("{appId}", System.Uri.EscapeDataString(ConvertToString(appId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<Role>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<SecureObjectDto>> SecureObjectsAsync(string appId, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (appId == null)
                throw new System.ArgumentNullException("appId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps/{appId}/secureObjects");
            urlBuilder_.Replace("{appId}", System.Uri.EscapeDataString(ConvertToString(appId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<System.Collections.Generic.ICollection<SecureObjectDto>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<RoleUser>> RoleUsersAsync(string appId, System.Guid? roleId = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (appId == null)
                throw new System.ArgumentNullException("appId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append("api/apps/{appId}/role-users?");
            urlBuilder_.Replace("{appId}", System.Uri.EscapeDataString(ConvertToString(appId, System.Globalization.CultureInfo.InvariantCulture)));
            if (roleId != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("roleId") + "=").Append(System.Uri.EscapeDataString(ConvertToString(roleId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Content = new System.Net.Http.StringContent(string.Empty, System.Text.Encoding.UTF8, "text/plain");
                    request_.Method = new System.Net.Http.HttpMethod("POST");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<System.Collections.Generic.ICollection<RoleUser>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            public T Object { get; }

            public string Text { get; }
        }

        public bool ReadResponseAsString { get; set; }

        protected virtual async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(System.Net.Http.HttpResponseMessage response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Threading.CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            if (ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = System.Text.Json.JsonSerializer.Deserialize<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (System.Text.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        var typedBody = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(responseStream, JsonSerializerSettings, cancellationToken).ConfigureAwait(false);
                        return new ObjectResponseResult<T>(typedBody, string.Empty);
                    }
                }
                catch (System.Text.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }

        private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is System.Enum)
            {
                var name = System.Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }

                    var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
            {
                return System.Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return System.Convert.ToBase64String((byte[])value);
            }
            else if (value.GetType().IsArray)
            {
                var array = System.Linq.Enumerable.OfType<object>((System.Array)value);
                return string.Join(",", System.Linq.Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }

            var result = System.Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class App
    {

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("appName")]
        public string AppName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("appDescription")]
        public string AppDescription { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectTypes")]
        public System.Collections.Generic.ICollection<SecureObjectType> SecureObjectTypes { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroups")]
        public System.Collections.Generic.ICollection<PermissionGroup> PermissionGroups { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("groupPermissions")]
        public System.Collections.Generic.ICollection<PermissionGroupPermission> GroupPermissions { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissions")]
        public System.Collections.Generic.ICollection<Permission> Permissions { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("roles")]
        public System.Collections.Generic.ICollection<Role> Roles { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class AppCreateRequest
    {

        [System.Text.Json.Serialization.JsonPropertyName("appName")]
        public string AppName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("appDescription")]
        public string AppDescription { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class AppDto
    {

        [System.Text.Json.Serialization.JsonPropertyName("appName")]
        public string AppName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("appDescription")]
        public string AppDescription { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("systemSecureObjectId")]
        public System.Guid SystemSecureObjectId { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class AppInitRequest
    {

        [System.Text.Json.Serialization.JsonPropertyName("rootSecureObjectId")]
        public System.Guid RootSecureObjectId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectTypes")]
        public System.Collections.Generic.ICollection<SecureObjectTypeDto> SecureObjectTypes { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissions")]
        public System.Collections.Generic.ICollection<PermissionDto> Permissions { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroups")]
        public System.Collections.Generic.ICollection<PermissionGroupDto> PermissionGroups { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("removeOtherPermissionGroups")]
        public bool RemoveOtherPermissionGroups { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class Permission
    {

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionId")]
        public int PermissionId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionName")]
        public string PermissionName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("app")]
        public App App { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroups")]
        public System.Collections.Generic.ICollection<PermissionGroup> PermissionGroups { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroupPermissions")]
        public System.Collections.Generic.ICollection<PermissionGroupPermission> PermissionGroupPermissions { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class PermissionDto
    {

        [System.Text.Json.Serialization.JsonPropertyName("permissionId")]
        public int PermissionId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionName")]
        public string PermissionName { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class PermissionGroup
    {

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroupId")]
        public int PermissionGroupId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroupExternalId")]
        public System.Guid PermissionGroupExternalId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroupName")]
        public string PermissionGroupName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissions")]
        public System.Collections.Generic.ICollection<Permission> Permissions { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("app")]
        public App App { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class PermissionGroupDto
    {

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroupId")]
        public System.Guid PermissionGroupId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroupName")]
        public string PermissionGroupName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissions")]
        public System.Collections.Generic.ICollection<PermissionDto> Permissions { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class PermissionGroupPermission
    {

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroupId")]
        public int PermissionGroupId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionId")]
        public int PermissionId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroup")]
        public PermissionGroup PermissionGroup { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permission")]
        public Permission Permission { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("app")]
        public App App { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class Role
    {

        [System.Text.Json.Serialization.JsonPropertyName("roleId")]
        public System.Guid RoleId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ownerId")]
        public System.Guid OwnerId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("roleName")]
        public string RoleName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("modifiedByUserId")]
        public System.Guid ModifiedByUserId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("createdTime")]
        public System.DateTime CreatedTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("rolePermissions")]
        public System.Collections.Generic.ICollection<SecureObjectRolePermission> RolePermissions { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("roleUsers")]
        public System.Collections.Generic.ICollection<RoleUser> RoleUsers { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class RoleUser
    {

        [System.Text.Json.Serialization.JsonPropertyName("roleId")]
        public System.Guid RoleId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("userId")]
        public System.Guid UserId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("modifiedByUserId")]
        public System.Guid ModifiedByUserId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("createdTime")]
        public System.DateTime CreatedTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("role")]
        public Role Role { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class SecureObject
    {

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectId")]
        public int SecureObjectId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectTypeId")]
        public int SecureObjectTypeId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("parentSecureObjectId")]
        public int? ParentSecureObjectId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectExternalId")]
        public System.Guid SecureObjectExternalId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectType")]
        public SecureObjectType SecureObjectType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("rolePermissions")]
        public System.Collections.Generic.ICollection<SecureObjectRolePermission> RolePermissions { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("userPermissions")]
        public System.Collections.Generic.ICollection<SecureObjectUserPermission> UserPermissions { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class SecureObjectDto
    {

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectId")]
        public System.Guid SecureObjectId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectTypeId")]
        public System.Guid SecureObjectTypeId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("parentSecureObjectId")]
        public System.Guid? ParentSecureObjectId { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class SecureObjectRolePermission
    {

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectId")]
        public int SecureObjectId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("roleId")]
        public System.Guid RoleId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroupId")]
        public int PermissionGroupId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("modifiedByUserId")]
        public System.Guid ModifiedByUserId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("createdTime")]
        public System.DateTime CreatedTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObject")]
        public SecureObject SecureObject { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("role")]
        public Role Role { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroup")]
        public PermissionGroup PermissionGroup { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class SecureObjectType
    {

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectTypeId")]
        public int SecureObjectTypeId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectTypeExternalId")]
        public System.Guid SecureObjectTypeExternalId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectTypeName")]
        public string SecureObjectTypeName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("app")]
        public App App { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isSystem")]
        public bool IsSystem { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjects")]
        public System.Collections.Generic.ICollection<SecureObject> SecureObjects { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class SecureObjectTypeDto
    {

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectTypeName")]
        public string SecureObjectTypeName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectTypeId")]
        public System.Guid SecureObjectTypeId { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class SecureObjectUserPermission
    {

        [System.Text.Json.Serialization.JsonPropertyName("appId")]
        public int AppId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("secureObjectId")]
        public int SecureObjectId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("userId")]
        public System.Guid UserId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("permissionGroupId")]
        public int PermissionGroupId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("modifiedByUserId")]
        public System.Guid ModifiedByUserId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("createdTime")]
        public System.DateTime CreatedTime { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class UserDto
    {

        [System.Text.Json.Serialization.JsonPropertyName("userId")]
        public System.Guid UserId { get; set; }

    }



    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ApiException : System.Exception
    {
        public int StatusCode { get; private set; }

        public string Response { get; private set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Exception innerException)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null) ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.15.7.0 (NJsonSchema v10.6.7.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ApiException<TResult> : ApiException
    {
        public TResult Result { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, TResult result, System.Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }

}