[![Build Status](https://dev.azure.com/Gekiss/Abc.Xacml/_apis/build/status/abc-software.abc.xacml)](https://dev.azure.com/Gekiss/Abc.Xacml/_build/latest?definitionId=1)
# abc.xacml 

This is a .NET library was to implement the [XACML](http://www.oasis-open.org/committees/tc_home.php?wg_abbrev=xacml) specification released by [Oasis](http://www.oasis-open.org/home/index.php). 
Library is under the GNU LGPLv3 licence. 

The XACML defines a declarative access control policy language implemented in XML and a processing model describing how to evaluate access requests according to the rules defined in policies. 

Features:
* This code implements XACML 1.0/1.1/2.0/3.0-wd17
* Extensible type systems
* Extensible functions
* Extensible algorithms
* Extensible XPath versions
* Policy repositories

### Install with NuGet package manager
[![NuGet status](https://img.shields.io/nuget/v/Abc.Xacml.png)](https://www.nuget.org/packages/Abc.Xacml)
```
PM> Install-Package Abc.Xacml
```

# abc.geoxacml
This is a .NET library was to implement the [GeoXACML](http://www.opengeospatial.org/standards/geoxacml) specification released by [OGC](http://www.opengeospatial.org/).

Features:
* This code implements GeoXACML 1.0.0

### Install with NuGet package manager
[![NuGet status](https://img.shields.io/nuget/v/Abc.Xacml.Geo.png)](https://www.nuget.org/packages/Abc.Xacml.Geo)
```
PM> Install-Package Abc.Xacml.Geo
```

## Requirements
You'll need .NET Framework 4.0 or later to use the precompiled binaries. To build library, you'll need Visual Studio 2017. To run tests, you'll need NUnit Test Adapter.
