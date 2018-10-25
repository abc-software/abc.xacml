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
[![NuGet](https://img.shields.io/badge/nuget-v1.2.0-blue.svg)](http://nuget.abcsoftware.lv/nuget/Packages(Id='Abc.Xacml',Version='1.2.0'))
```
PM> Install-Package Abc.Xacml
```

# abc.geoxacml
This is a .NET library was to implement the [GeoXACML](http://www.opengeospatial.org/standards/geoxacml) specification released by [OGC](http://www.opengeospatial.org/).

Features:
* This code implements GeoXACML 1.0.0

### Install with NuGet package manager
[![NuGet](https://img.shields.io/badge/nuget-v1.2.0-blue.svg)](http://nuget.abcsoftware.lv/nuget/Packages(Id='Abc.Xacml.Geo',Version='1.2.0'))
```
PM> Install-Package Abc.Xacml.Geo
```

## Requirements
You'll need .NET Framework 4.0 or later to use the precompiled binaries. To build library, you'll need Visual Studio 2017. To run tests, you'll need NUnit Test Adapter.
