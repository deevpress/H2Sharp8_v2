# H2Sharp8_v14200

H2Sharp8_v14200 is a fork of the original H2Sharp project, providing an ADO.NET wrapper for the H2 Database Engine (version 1.4.200) optimized for .NET 8. This project enables seamless integration of H2, a Java-based relational database, into .NET applications.

## Overview
H2 is a lightweight, embeddable, open-source relational database written in Java. This fork uses IKVM.NET (version 8.14.0) to convert the H2 1.4.200 JAR file into .NET Common Intermediate Language (CIL), creating a managed library (`H2_14200_Net8.dll`). The wrapper classes implement the ADO.NET interface, making it easy to use H2 in .NET 8 projects.

## Key Features
- **.NET 8 Support**: Fully compatible with the latest .NET platform.
- **H2 1.4.200**: Utilizes a specific, stable version of H2 (note: H2 2.0 is not supported in this fork due to compatibility differences).

## Requirements
- .NET 8 SDK
- IKVM 8.14.0 (available via NuGet)
- H2Sharp8_v14200 NuGet package (includes H2_14200_Net8.dll compiled from h2-1.4.200.jar)

## Build Instructions

git clone https://github.com/deevpress/H2Sharp8_v14200.git
cd H2Sharp8_v14200
dotnet build