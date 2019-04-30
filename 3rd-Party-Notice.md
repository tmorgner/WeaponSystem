# 3rd party assets used in this project

## NaughtyAttributes

NaughtyAttributes is an extension for the Unity Inspector.

It expands the range of attributes that Unity provides so that you can 
create powerful inspectors without the need of custom editors or property 
drawers. It also provides attributes that can be applied to non-serialized 
fields or functions.

It is implemented by replacing the default Unity Inspector. This means 
that if you have any custom editors, NaughtyAttributes will not work with 
them. All of your custom editors and property drawers are not affected 
in any way.

* License: MIT
* Source: https://github.com/dbrizov/NaughtyAttributes

## FluentAssertions

Fluent API for asserting the results of unit tests that targets .NET 
Framework 4.5, 4.7, .NET Standard 1.3, 1.6 and 2.0. Supports the unit 
test frameworks MSTest, MSTest2, Gallio, NUnit, XUnit, MBunit, MSpec, 
and NSpec. 

The version contained here is stripped down to remove all parts of the
library that are inherently incompatible with Unity's interpretation
of C# and their runtime libraries.

* License: Apache License 2.0
* Source: https://github.com/fluentassertions/fluentassertions/
