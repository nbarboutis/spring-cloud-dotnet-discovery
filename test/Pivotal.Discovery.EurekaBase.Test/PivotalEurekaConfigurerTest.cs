﻿// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Configuration;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.Discovery.Eureka;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.IO;
using Xunit;

namespace Pivotal.Discovery.Eureka.Client.Test
{
    public class PivotalEurekaConfigurerTest
    {
        [Fact]
        public void UpdateConfiguration_NoServiceInfo_ConfiguresEurekaDiscovery_Correctly()
        {
            // Arrange
            var appsettings = @"
{
'eureka': {
    'client': {
        'eurekaServer': {
            'proxyHost': 'proxyHost',
            'proxyPort': 100,
            'proxyUserName': 'proxyUserName',
            'proxyPassword': 'proxyPassword',
            'shouldGZipContent': true,
            'connectTimeoutSeconds': 100
        },
        'allowRedirects': true,
        'shouldDisableDelta': true,
        'shouldFilterOnlyUpInstances': true,
        'shouldFetchRegistry': true,
        'registryRefreshSingleVipAddress':'registryRefreshSingleVipAddress',
        'shouldOnDemandUpdateStatusChange': true,
        'shouldRegisterWithEureka': true,
        'registryFetchIntervalSeconds': 100,
        'instanceInfoReplicationIntervalSeconds': 100,
        'serviceUrl': 'http://localhost:8761/eureka/'
    },
    'instance': {
        'instanceId': 'instanceId',
        'appName': 'appName',
        'appGroup': 'appGroup',
        'instanceEnabledOnInit': true,
        'hostname': 'hostname',
        'port': 100,
        'securePort': 100,
        'nonSecurePortEnabled': true,
        'securePortEnabled': true,
        'leaseExpirationDurationInSeconds':100,
        'leaseRenewalIntervalInSeconds': 100,
        'secureVipAddress': 'secureVipAddress',
        'vipAddress': 'vipAddress',
        'asgName': 'asgName',
        'metadataMap': {
            'foo': 'bar',
            'bar': 'foo'
        },
        'statusPageUrlPath': 'statusPageUrlPath',
        'statusPageUrl': 'statusPageUrl',
        'homePageUrlPath':'homePageUrlPath',
        'homePageUrl': 'homePageUrl',
        'healthCheckUrlPath': 'healthCheckUrlPath',
        'healthCheckUrl':'healthCheckUrl',
        'secureHealthCheckUrl':'secureHealthCheckUrl'   
    }
    }
}";
            var basePath = Path.GetTempPath();
            var path = TestHelpers.CreateTempFile(appsettings);
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(basePath);
            configurationBuilder.AddJsonFile(Path.GetFileName(path));
            var config = configurationBuilder.Build();

            var clientOpts = new EurekaClientOptions();
            var clientSection = config.GetSection(EurekaClientOptions.EUREKA_CLIENT_CONFIGURATION_PREFIX);
            clientSection.Bind(clientOpts);

            PivotalEurekaConfigurer.UpdateConfiguration(config, null, clientOpts);

            var co = clientOpts;
            Assert.NotNull(co);
            Assert.Equal("proxyHost", co.ProxyHost);
            Assert.Equal(100, co.ProxyPort);
            Assert.Equal("proxyPassword", co.ProxyPassword);
            Assert.Equal("proxyUserName", co.ProxyUserName);
            Assert.True(co.AllowRedirects);
            Assert.Equal(100, co.InstanceInfoReplicationIntervalSeconds);
            Assert.Equal(100, co.EurekaServerConnectTimeoutSeconds);
            Assert.Equal("http://localhost:8761/eureka/", co.EurekaServerServiceUrls);
            Assert.Equal(100, co.RegistryFetchIntervalSeconds);
            Assert.Equal("registryRefreshSingleVipAddress", co.RegistryRefreshSingleVipAddress);
            Assert.True(co.ShouldDisableDelta);
            Assert.True(co.ShouldFetchRegistry);
            Assert.True(co.ShouldFilterOnlyUpInstances);
            Assert.True(co.ShouldGZipContent);
            Assert.True(co.ShouldOnDemandUpdateStatusChange);
            Assert.True(co.ShouldRegisterWithEureka);

            var instOpts = new EurekaInstanceOptions();
            var instSection = config.GetSection(EurekaInstanceOptions.EUREKA_INSTANCE_CONFIGURATION_PREFIX);
            instSection.Bind(instOpts);

            PivotalEurekaConfigurer.UpdateConfiguration(config, null, instOpts);

            EurekaInstanceOptions ro = instOpts;

            Assert.Equal("instanceId", ro.InstanceId);
            Assert.Equal("appName", ro.AppName);
            Assert.Equal("appGroup", ro.AppGroupName);
            Assert.True(ro.IsInstanceEnabledOnInit);
            Assert.Equal(100, ro.NonSecurePort);
            Assert.Equal("hostname", ro.HostName);
            Assert.Equal(100, ro.SecurePort);
            Assert.True(ro.IsNonSecurePortEnabled);
            Assert.True(ro.SecurePortEnabled);
            Assert.Equal(100, ro.LeaseExpirationDurationInSeconds);
            Assert.Equal(100, ro.LeaseRenewalIntervalInSeconds);
            Assert.Equal("secureVipAddress", ro.SecureVirtualHostName);
            Assert.Equal("vipAddress", ro.VirtualHostName);
            Assert.Equal("asgName", ro.ASGName);

            Assert.Equal("statusPageUrlPath", ro.StatusPageUrlPath);
            Assert.Equal("statusPageUrl", ro.StatusPageUrl);
            Assert.Equal("homePageUrlPath", ro.HomePageUrlPath);
            Assert.Equal("homePageUrl", ro.HomePageUrl);
            Assert.Equal("healthCheckUrlPath", ro.HealthCheckUrlPath);
            Assert.Equal("healthCheckUrl", ro.HealthCheckUrl);
            Assert.Equal("secureHealthCheckUrl", ro.SecureHealthCheckUrl);

            var map = ro.MetadataMap;
            Assert.NotNull(map);
            Assert.Equal(2, map.Count);
            Assert.Equal("bar", map["foo"]);
            Assert.Equal("foo", map["bar"]);
        }

        [Fact]
        public void UpdateConfiguration_WithVCAPEnvVariables_HostName_ConfiguresEurekaDiscovery_Correctly()
        {
            var vcap_application = @"
{
    'limits': {
    'fds': 16384,
    'mem': 512,
    'disk': 1024
    },
    'application_name': 'foo',
    'application_uris': [
    'foo.apps.testcloud.com'
    ],
    'name': 'foo',
    'space_name': 'test',
    'space_id': '98c627e7-f559-46a4-9032-88cab63f8249',
    'uris': [
    'foo.apps.testcloud.com'
    ],
    'users': null,
    'version': '4a439db9-4a82-47a3-aeea-8240465cff8e',
    'application_version': '4a439db9-4a82-47a3-aeea-8240465cff8e',
    'application_id': 'ac923014-93a5-4aee-b934-a043b241868b',
    'instance_id': 'instance_id'

}";
            var vcap_services = @"
{
'p-config-server': [
    {
    'credentials': {
        'uri': 'https://config-de211817-2e99-4c57-89e8-31fa7ca6a276.apps.testcloud.com',
        'client_id': 'p-config-server-8f49dd26-e6cd-47a6-b2a0-7655cea20333',
        'client_secret': 'vBDjqIf7XthT',
        'access_token_uri': 'https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token'
    },
    'syslog_drain_url': null,
    'label': 'p-config-server',
    'provider': null,
    'plan': 'standard',
    'name': 'myConfigServer',
    'tags': [
        'configuration',
        'spring-cloud'
    ]
    }
    ],
'p-service-registry': [
{
    'credentials': {
        'uri': 'https://eureka-6a1b81f5-79e2-4d14-a86b-ddf584635a60.apps.testcloud.com',
        'client_id': 'p-service-registry-06e28efd-24be-4ce3-9784-854ed8d2acbe',
        'client_secret': 'dCsdoiuklicS',
        'access_token_uri': 'https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token'
        },
    'syslog_drain_url': null,
    'label': 'p-service-registry',
    'provider': null,
    'plan': 'standard',
    'name': 'myDiscoveryService',
    'tags': [
    'eureka',
    'discovery',
    'registry',
    'spring-cloud'
    ]
}
]
}";

            var appsettings = @"
{
'spring': {
    'cloud': {
        'discovery': {
            'registrationMethod': 'hostname'
        }
    }
},
'eureka': {
    'client': {
        'eurekaServer': {
            'proxyHost': 'proxyHost',
            'proxyPort': 100,
            'proxyUserName': 'proxyUserName',
            'proxyPassword': 'proxyPassword',
            'shouldGZipContent': true,
            'connectTimeoutSeconds': 100
        },
        'allowRedirects': true,
        'shouldDisableDelta': true,
        'shouldFilterOnlyUpInstances': true,
        'shouldFetchRegistry': true,
        'registryRefreshSingleVipAddress':'registryRefreshSingleVipAddress',
        'shouldOnDemandUpdateStatusChange': true,
        'shouldRegisterWithEureka': true,
        'registryFetchIntervalSeconds': 100,
        'instanceInfoReplicationIntervalSeconds': 100,
        'serviceUrl': 'http://localhost:8761/eureka/'
    },
    'instance': {
        'instanceId': 'instanceId',
        'appGroup': 'appGroup',
        'instanceEnabledOnInit': true,
        'hostname': 'myhostname',
        'port': 100,
        'securePort': 100,
        'nonSecurePortEnabled': true,
        'securePortEnabled': true,
        'leaseExpirationDurationInSeconds':100,
        'leaseRenewalIntervalInSeconds': 100,
        'secureVipAddress': 'secureVipAddress',
        'vipAddress': 'vipAddress',
        'asgName': 'asgName',
        'metadataMap': {
            'foo': 'bar',
            'bar': 'foo'
        },
        'statusPageUrlPath': 'statusPageUrlPath',
        'statusPageUrl': 'statusPageUrl',
        'homePageUrlPath':'homePageUrlPath',
        'homePageUrl': 'homePageUrl',
        'healthCheckUrlPath': 'healthCheckUrlPath',
        'healthCheckUrl':'healthCheckUrl',
        'secureHealthCheckUrl':'secureHealthCheckUrl'   
    }
    }
}";
            Environment.SetEnvironmentVariable("VCAP_APPLICATION", vcap_application);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcap_services);
            Environment.SetEnvironmentVariable("CF_INSTANCE_INDEX", "1");
            Environment.SetEnvironmentVariable("CF_INSTANCE_GUID", "ac923014-93a5-4aee-b934-a043b241868b");

            var path = TestHelpers.CreateTempFile(appsettings);
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);

            configurationBuilder.AddJsonFile(fileName);
            configurationBuilder.AddCloudFoundry();
            var config = configurationBuilder.Build();

            var sis = config.GetServiceInfos<EurekaServiceInfo>();
            Assert.Single(sis);
            EurekaServiceInfo si = sis[0];

            var clientOpts = new EurekaClientOptions();
            var clientSection = config.GetSection(EurekaClientOptions.EUREKA_CLIENT_CONFIGURATION_PREFIX);
            clientSection.Bind(clientOpts);

            PivotalEurekaConfigurer.UpdateConfiguration(config, si, clientOpts);

            var co = clientOpts;
            Assert.NotNull(co);
            Assert.Equal("proxyHost", co.ProxyHost);
            Assert.Equal(100, co.ProxyPort);
            Assert.Equal("proxyPassword", co.ProxyPassword);
            Assert.Equal("proxyUserName", co.ProxyUserName);
            Assert.True(co.AllowRedirects);
            Assert.Equal(100, co.InstanceInfoReplicationIntervalSeconds);
            Assert.Equal(100, co.EurekaServerConnectTimeoutSeconds);
            Assert.Equal("https://eureka-6a1b81f5-79e2-4d14-a86b-ddf584635a60.apps.testcloud.com/eureka/", co.EurekaServerServiceUrls);
            Assert.Equal(100, co.RegistryFetchIntervalSeconds);
            Assert.Equal("registryRefreshSingleVipAddress", co.RegistryRefreshSingleVipAddress);
            Assert.True(co.ShouldDisableDelta);
            Assert.True(co.ShouldFetchRegistry);
            Assert.True(co.ShouldFilterOnlyUpInstances);
            Assert.True(co.ShouldGZipContent);
            Assert.True(co.ShouldOnDemandUpdateStatusChange);
            Assert.True(co.ShouldRegisterWithEureka);
            Assert.Equal("https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token", co.AccessTokenUri);
            Assert.Equal("p-service-registry-06e28efd-24be-4ce3-9784-854ed8d2acbe", co.ClientId);
            Assert.Equal("dCsdoiuklicS", co.ClientSecret);

            var instOpts = new EurekaInstanceOptions();
            var instSection = config.GetSection(EurekaInstanceOptions.EUREKA_INSTANCE_CONFIGURATION_PREFIX);
            instSection.Bind(instOpts);

            PivotalEurekaConfigurer.UpdateConfiguration(config, si, instOpts);

            var ro = instOpts;

            Assert.Equal("hostname", ro.RegistrationMethod);
            Assert.Equal("myhostname:instance_id", ro.InstanceId);
            Assert.Equal("foo", ro.AppName);
            Assert.Equal("appGroup", ro.AppGroupName);
            Assert.True(ro.IsInstanceEnabledOnInit);
            Assert.Equal(100, ro.NonSecurePort);
            Assert.Equal("myhostname", ro.HostName);
            Assert.Equal(100, ro.SecurePort);
            Assert.True(ro.IsNonSecurePortEnabled);
            Assert.True(ro.SecurePortEnabled);
            Assert.Equal(100, ro.LeaseExpirationDurationInSeconds);
            Assert.Equal(100, ro.LeaseRenewalIntervalInSeconds);
            Assert.Equal("secureVipAddress", ro.SecureVirtualHostName);
            Assert.Equal("vipAddress", ro.VirtualHostName);
            Assert.Equal("asgName", ro.ASGName);

            Assert.Equal("statusPageUrlPath", ro.StatusPageUrlPath);
            Assert.Equal("statusPageUrl", ro.StatusPageUrl);
            Assert.Equal("homePageUrlPath", ro.HomePageUrlPath);
            Assert.Equal("homePageUrl", ro.HomePageUrl);
            Assert.Equal("healthCheckUrlPath", ro.HealthCheckUrlPath);
            Assert.Equal("healthCheckUrl", ro.HealthCheckUrl);
            Assert.Equal("secureHealthCheckUrl", ro.SecureHealthCheckUrl);

            var map = ro.MetadataMap;
            Assert.NotNull(map);
            Assert.Equal(6, map.Count);
            Assert.Equal("bar", map["foo"]);
            Assert.Equal("foo", map["bar"]);
            Assert.Equal("instance_id", map[PivotalEurekaConfigurer.INSTANCE_ID]);
            Assert.Equal("ac923014-93a5-4aee-b934-a043b241868b", map[PivotalEurekaConfigurer.CF_APP_GUID]);
            Assert.Equal("1", map[PivotalEurekaConfigurer.CF_INSTANCE_INDEX]);
            Assert.Equal(PivotalEurekaConfigurer.UNKNOWN_ZONE, map[PivotalEurekaConfigurer.ZONE]);
        }

        [Fact]
        public void UpdateConfiguration_WithVCAPEnvVariables_Route_ConfiguresEurekaDiscovery_Correctly()
        {
            var vcap_application = @"
{
    'limits': {
    'fds': 16384,
    'mem': 512,
    'disk': 1024
    },
    'application_name': 'foo',
    'application_uris': [
    'foo.apps.testcloud.com'
    ],
    'name': 'foo',
    'space_name': 'test',
    'space_id': '98c627e7-f559-46a4-9032-88cab63f8249',
    'uris': [
    'foo.apps.testcloud.com'
    ],
    'users': null,
    'version': '4a439db9-4a82-47a3-aeea-8240465cff8e',
    'application_version': '4a439db9-4a82-47a3-aeea-8240465cff8e',
    'application_id': 'ac923014-93a5-4aee-b934-a043b241868b',
    'instance_id': 'instance_id'

}";
            var vcap_services = @"
{
'p-config-server': [
    {
    'credentials': {
        'uri': 'https://config-de211817-2e99-4c57-89e8-31fa7ca6a276.apps.testcloud.com',
        'client_id': 'p-config-server-8f49dd26-e6cd-47a6-b2a0-7655cea20333',
        'client_secret': 'vBDjqIf7XthT',
        'access_token_uri': 'https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token'
    },
    'syslog_drain_url': null,
    'label': 'p-config-server',
    'provider': null,
    'plan': 'standard',
    'name': 'myConfigServer',
    'tags': [
        'configuration',
        'spring-cloud'
    ]
    }
    ],
'p-service-registry': [
{
    'credentials': {
        'uri': 'https://eureka-6a1b81f5-79e2-4d14-a86b-ddf584635a60.apps.testcloud.com',
        'client_id': 'p-service-registry-06e28efd-24be-4ce3-9784-854ed8d2acbe',
        'client_secret': 'dCsdoiuklicS',
        'access_token_uri': 'https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token'
        },
    'syslog_drain_url': null,
    'label': 'p-service-registry',
    'provider': null,
    'plan': 'standard',
    'name': 'myDiscoveryService',
    'tags': [
    'eureka',
    'discovery',
    'registry',
    'spring-cloud'
    ]
}
]
}";

            var appsettings = @"
{
'eureka': {
    'client': {
        'eurekaServer': {
            'proxyHost': 'proxyHost',
            'proxyPort': 100,
            'proxyUserName': 'proxyUserName',
            'proxyPassword': 'proxyPassword',
            'shouldGZipContent': true,
            'connectTimeoutSeconds': 100
        },
        'allowRedirects': true,
        'shouldDisableDelta': true,
        'shouldFilterOnlyUpInstances': true,
        'shouldFetchRegistry': true,
        'registryRefreshSingleVipAddress':'registryRefreshSingleVipAddress',
        'shouldOnDemandUpdateStatusChange': true,
        'shouldRegisterWithEureka': true,
        'registryFetchIntervalSeconds': 100,
        'instanceInfoReplicationIntervalSeconds': 100,
        'serviceUrl': 'http://localhost:8761/eureka/'
    },
    'instance': {
        'registrationMethod': 'route',
        'instanceId': 'instanceId',
        'appGroup': 'appGroup',
        'instanceEnabledOnInit': true,
        'hostname': 'myhostname',
        'port': 100,
        'securePort': 100,
        'nonSecurePortEnabled': true,
        'securePortEnabled': true,
        'leaseExpirationDurationInSeconds':100,
        'leaseRenewalIntervalInSeconds': 100,
        'secureVipAddress': 'secureVipAddress',
        'vipAddress': 'vipAddress',
        'asgName': 'asgName',
        'metadataMap': {
            'foo': 'bar',
            'bar': 'foo'
        },
        'statusPageUrlPath': 'statusPageUrlPath',
        'statusPageUrl': 'statusPageUrl',
        'homePageUrlPath':'homePageUrlPath',
        'homePageUrl': 'homePageUrl',
        'healthCheckUrlPath': 'healthCheckUrlPath',
        'healthCheckUrl':'healthCheckUrl',
        'secureHealthCheckUrl':'secureHealthCheckUrl'   
    }
    }
}";
            Environment.SetEnvironmentVariable("VCAP_APPLICATION", vcap_application);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcap_services);
            Environment.SetEnvironmentVariable("CF_INSTANCE_INDEX", "1");
            Environment.SetEnvironmentVariable("CF_INSTANCE_GUID", "ac923014-93a5-4aee-b934-a043b241868b");
            var path = TestHelpers.CreateTempFile(appsettings);
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);

            configurationBuilder.AddJsonFile(fileName);
            configurationBuilder.AddCloudFoundry();
            var config = configurationBuilder.Build();

            var sis = config.GetServiceInfos<EurekaServiceInfo>();
            Assert.Single(sis);
            EurekaServiceInfo si = sis[0];

            var clientOpts = new EurekaClientOptions();
            var clientSection = config.GetSection(EurekaClientOptions.EUREKA_CLIENT_CONFIGURATION_PREFIX);
            clientSection.Bind(clientOpts);

            PivotalEurekaConfigurer.UpdateConfiguration(config, si, clientOpts);

            var co = clientOpts;
            Assert.NotNull(co);
            Assert.Equal("proxyHost", co.ProxyHost);
            Assert.Equal(100, co.ProxyPort);
            Assert.Equal("proxyPassword", co.ProxyPassword);
            Assert.Equal("proxyUserName", co.ProxyUserName);
            Assert.True(co.AllowRedirects);
            Assert.Equal(100, co.InstanceInfoReplicationIntervalSeconds);
            Assert.Equal(100, co.EurekaServerConnectTimeoutSeconds);
            Assert.Equal("https://eureka-6a1b81f5-79e2-4d14-a86b-ddf584635a60.apps.testcloud.com/eureka/", co.EurekaServerServiceUrls);
            Assert.Equal(100, co.RegistryFetchIntervalSeconds);
            Assert.Equal("registryRefreshSingleVipAddress", co.RegistryRefreshSingleVipAddress);
            Assert.True(co.ShouldDisableDelta);
            Assert.True(co.ShouldFetchRegistry);
            Assert.True(co.ShouldFilterOnlyUpInstances);
            Assert.True(co.ShouldGZipContent);
            Assert.True(co.ShouldOnDemandUpdateStatusChange);
            Assert.True(co.ShouldRegisterWithEureka);
            Assert.Equal("https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token", co.AccessTokenUri);
            Assert.Equal("p-service-registry-06e28efd-24be-4ce3-9784-854ed8d2acbe", co.ClientId);
            Assert.Equal("dCsdoiuklicS", co.ClientSecret);

            var instOpts = new EurekaInstanceOptions();
            var instSection = config.GetSection(EurekaInstanceOptions.EUREKA_INSTANCE_CONFIGURATION_PREFIX);
            instSection.Bind(instOpts);

            PivotalEurekaConfigurer.UpdateConfiguration(config, si, instOpts);

            var ro = instOpts;

            Assert.Equal("route", ro.RegistrationMethod);
            Assert.Equal("foo.apps.testcloud.com:instance_id", ro.InstanceId);
            Assert.Equal("foo", ro.AppName);
            Assert.Equal("appGroup", ro.AppGroupName);
            Assert.True(ro.IsInstanceEnabledOnInit);
            Assert.Equal(80, ro.NonSecurePort);
            Assert.Equal("foo.apps.testcloud.com", ro.HostName);
            Assert.Equal(443, ro.SecurePort);
            Assert.True(ro.IsNonSecurePortEnabled);
            Assert.True(ro.SecurePortEnabled);
            Assert.Equal(100, ro.LeaseExpirationDurationInSeconds);
            Assert.Equal(100, ro.LeaseRenewalIntervalInSeconds);
            Assert.Equal("secureVipAddress", ro.SecureVirtualHostName);
            Assert.Equal("vipAddress", ro.VirtualHostName);
            Assert.Equal("asgName", ro.ASGName);

            Assert.Equal("statusPageUrlPath", ro.StatusPageUrlPath);
            Assert.Equal("statusPageUrl", ro.StatusPageUrl);
            Assert.Equal("homePageUrlPath", ro.HomePageUrlPath);
            Assert.Equal("homePageUrl", ro.HomePageUrl);
            Assert.Equal("healthCheckUrlPath", ro.HealthCheckUrlPath);
            Assert.Equal("healthCheckUrl", ro.HealthCheckUrl);
            Assert.Equal("secureHealthCheckUrl", ro.SecureHealthCheckUrl);

            var map = ro.MetadataMap;
            Assert.NotNull(map);
            Assert.Equal(6, map.Count);
            Assert.Equal("bar", map["foo"]);
            Assert.Equal("foo", map["bar"]);
            Assert.Equal("instance_id", map[PivotalEurekaConfigurer.INSTANCE_ID]);
            Assert.Equal("ac923014-93a5-4aee-b934-a043b241868b", map[PivotalEurekaConfigurer.CF_APP_GUID]);
            Assert.Equal("1", map[PivotalEurekaConfigurer.CF_INSTANCE_INDEX]);
            Assert.Equal(PivotalEurekaConfigurer.UNKNOWN_ZONE, map[PivotalEurekaConfigurer.ZONE]);
        }

        [Fact]
        public void UpdateConfiguration_WithVCAPEnvVariables_AppName_Overrides_VCAPBinding()
        {
            var vcap_application = @"
{
    'limits': {
    'fds': 16384,
    'mem': 512,
    'disk': 1024
    },
    'application_name': 'foo',
    'application_uris': [
    'foo.apps.testcloud.com'
    ],
    'name': 'foo',
    'space_name': 'test',
    'space_id': '98c627e7-f559-46a4-9032-88cab63f8249',
    'uris': [
    'foo.apps.testcloud.com'
    ],
    'users': null,
    'version': '4a439db9-4a82-47a3-aeea-8240465cff8e',
    'application_version': '4a439db9-4a82-47a3-aeea-8240465cff8e',
    'application_id': 'ac923014-93a5-4aee-b934-a043b241868b',
    'instance_id': 'instance_id'

}";
            var vcap_services = @"
{
'p-config-server': [
    {
    'credentials': {
        'uri': 'https://config-de211817-2e99-4c57-89e8-31fa7ca6a276.apps.testcloud.com',
        'client_id': 'p-config-server-8f49dd26-e6cd-47a6-b2a0-7655cea20333',
        'client_secret': 'vBDjqIf7XthT',
        'access_token_uri': 'https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token'
    },
    'syslog_drain_url': null,
    'label': 'p-config-server',
    'provider': null,
    'plan': 'standard',
    'name': 'myConfigServer',
    'tags': [
        'configuration',
        'spring-cloud'
    ]
    }
    ],
'p-service-registry': [
{
    'credentials': {
        'uri': 'https://eureka-6a1b81f5-79e2-4d14-a86b-ddf584635a60.apps.testcloud.com',
        'client_id': 'p-service-registry-06e28efd-24be-4ce3-9784-854ed8d2acbe',
        'client_secret': 'dCsdoiuklicS',
        'access_token_uri': 'https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token'
        },
    'syslog_drain_url': null,
    'label': 'p-service-registry',
    'provider': null,
    'plan': 'standard',
    'name': 'myDiscoveryService',
    'tags': [
    'eureka',
    'discovery',
    'registry',
    'spring-cloud'
    ]
}
]
}";

            var appsettings = @"
{
'eureka': {
    'client': {
        'eurekaServer': {
            'proxyHost': 'proxyHost',
            'proxyPort': 100,
            'proxyUserName': 'proxyUserName',
            'proxyPassword': 'proxyPassword',
            'shouldGZipContent': true,
            'connectTimeoutSeconds': 100
        },
        'allowRedirects': true,
        'shouldDisableDelta': true,
        'shouldFilterOnlyUpInstances': true,
        'shouldFetchRegistry': true,
        'registryRefreshSingleVipAddress':'registryRefreshSingleVipAddress',
        'shouldOnDemandUpdateStatusChange': true,
        'shouldRegisterWithEureka': true,
        'registryFetchIntervalSeconds': 100,
        'instanceInfoReplicationIntervalSeconds': 100,
        'serviceUrl': 'http://localhost:8761/eureka/'
    },
    'instance': {
        'registrationMethod': 'hostname',
        'instanceId': 'instanceId',
        'appName': 'appName',
        'appGroup': 'appGroup',
        'instanceEnabledOnInit': true,
        'hostname': 'myhostname',
        'port': 100,
        'securePort': 100,
        'nonSecurePortEnabled': true,
        'securePortEnabled': true,
        'leaseExpirationDurationInSeconds':100,
        'leaseRenewalIntervalInSeconds': 100,
        'secureVipAddress': 'secureVipAddress',
        'vipAddress': 'vipAddress',
        'asgName': 'asgName',
        'metadataMap': {
            'foo': 'bar',
            'bar': 'foo'
        },
        'statusPageUrlPath': 'statusPageUrlPath',
        'statusPageUrl': 'statusPageUrl',
        'homePageUrlPath':'homePageUrlPath',
        'homePageUrl': 'homePageUrl',
        'healthCheckUrlPath': 'healthCheckUrlPath',
        'healthCheckUrl':'healthCheckUrl',
        'secureHealthCheckUrl':'secureHealthCheckUrl'   
    }
    }
}";
            Environment.SetEnvironmentVariable("VCAP_APPLICATION", vcap_application);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcap_services);
            Environment.SetEnvironmentVariable("CF_INSTANCE_INDEX", "1");
            Environment.SetEnvironmentVariable("CF_INSTANCE_GUID", "ac923014-93a5-4aee-b934-a043b241868b");
            var path = TestHelpers.CreateTempFile(appsettings);
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);

            configurationBuilder.AddJsonFile(fileName);
            configurationBuilder.AddCloudFoundry();
            var config = configurationBuilder.Build();

            var sis = config.GetServiceInfos<EurekaServiceInfo>();
            Assert.Single(sis);
            EurekaServiceInfo si = sis[0];

            var clientOpts = new EurekaClientOptions();
            var clientSection = config.GetSection(EurekaClientOptions.EUREKA_CLIENT_CONFIGURATION_PREFIX);
            clientSection.Bind(clientOpts);

            PivotalEurekaConfigurer.UpdateConfiguration(config, si, clientOpts);

            var co = clientOpts;

            Assert.NotNull(co);
            Assert.Equal("proxyHost", co.ProxyHost);
            Assert.Equal(100, co.ProxyPort);
            Assert.Equal("proxyPassword", co.ProxyPassword);
            Assert.Equal("proxyUserName", co.ProxyUserName);
            Assert.True(co.AllowRedirects);
            Assert.Equal(100, co.InstanceInfoReplicationIntervalSeconds);
            Assert.Equal(100, co.EurekaServerConnectTimeoutSeconds);
            Assert.Equal("https://eureka-6a1b81f5-79e2-4d14-a86b-ddf584635a60.apps.testcloud.com/eureka/", co.EurekaServerServiceUrls);
            Assert.Equal(100, co.RegistryFetchIntervalSeconds);
            Assert.Equal("registryRefreshSingleVipAddress", co.RegistryRefreshSingleVipAddress);
            Assert.True(co.ShouldDisableDelta);
            Assert.True(co.ShouldFetchRegistry);
            Assert.True(co.ShouldFilterOnlyUpInstances);
            Assert.True(co.ShouldGZipContent);
            Assert.True(co.ShouldOnDemandUpdateStatusChange);
            Assert.True(co.ShouldRegisterWithEureka);
            Assert.Equal("https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token", co.AccessTokenUri);
            Assert.Equal("p-service-registry-06e28efd-24be-4ce3-9784-854ed8d2acbe", co.ClientId);
            Assert.Equal("dCsdoiuklicS", co.ClientSecret);

            var instOpts = new EurekaInstanceOptions();
            var instSection = config.GetSection(EurekaInstanceOptions.EUREKA_INSTANCE_CONFIGURATION_PREFIX);
            instSection.Bind(instOpts);

            PivotalEurekaConfigurer.UpdateConfiguration(config, si, instOpts);

            var ro = instOpts;

            Assert.Equal("hostname", ro.RegistrationMethod);
            Assert.Equal("myhostname:instance_id", ro.InstanceId);
            Assert.Equal("appName", ro.AppName);
            Assert.Equal("appGroup", ro.AppGroupName);
            Assert.True(ro.IsInstanceEnabledOnInit);
            Assert.Equal(100, ro.NonSecurePort);
            Assert.Equal("myhostname", ro.HostName);
            Assert.Equal(100, ro.SecurePort);
            Assert.True(ro.IsNonSecurePortEnabled);
            Assert.True(ro.SecurePortEnabled);
            Assert.Equal(100, ro.LeaseExpirationDurationInSeconds);
            Assert.Equal(100, ro.LeaseRenewalIntervalInSeconds);
            Assert.Equal("secureVipAddress", ro.SecureVirtualHostName);
            Assert.Equal("vipAddress", ro.VirtualHostName);
            Assert.Equal("asgName", ro.ASGName);

            Assert.Equal("statusPageUrlPath", ro.StatusPageUrlPath);
            Assert.Equal("statusPageUrl", ro.StatusPageUrl);
            Assert.Equal("homePageUrlPath", ro.HomePageUrlPath);
            Assert.Equal("homePageUrl", ro.HomePageUrl);
            Assert.Equal("healthCheckUrlPath", ro.HealthCheckUrlPath);
            Assert.Equal("healthCheckUrl", ro.HealthCheckUrl);
            Assert.Equal("secureHealthCheckUrl", ro.SecureHealthCheckUrl);

            var map = ro.MetadataMap;
            Assert.NotNull(map);
            Assert.Equal(6, map.Count);
            Assert.Equal("bar", map["foo"]);
            Assert.Equal("foo", map["bar"]);
            Assert.Equal("instance_id", map[PivotalEurekaConfigurer.INSTANCE_ID]);
            Assert.Equal("ac923014-93a5-4aee-b934-a043b241868b", map[PivotalEurekaConfigurer.CF_APP_GUID]);
            Assert.Equal("1", map[PivotalEurekaConfigurer.CF_INSTANCE_INDEX]);
            Assert.Equal(PivotalEurekaConfigurer.UNKNOWN_ZONE, map[PivotalEurekaConfigurer.ZONE]);
        }
    }
}