
DnDGen.EventGen
============

This is a generic event logger that enqueues events and dequeues them on demand.  It allows for dynamic updates that can be accessed while other processes run - namely, the DnDGen.Web project can display detailed updates while other Gen projects are running.

[![Build Status](https://travis-ci.org/DnDGen/EventGen.svg?branch=master)](https://travis-ci.org/DnDGen/EventGen)

### Use

To use EventGen, simply use the GenEventQueue.

```C#
genEventQueue.Enqueue(clientID, "my generator", "I started generating");
var genEvent = genEventQueue.Dequeue(clientID);
```

### Getting the queue

You can obtain the event queue from the IoC namespace.

```C#
var kernel = new StandardKernel();
var eventGenModuleLoader = new EventGenModuleLoader();

eventGenModuleLoader.LoadModules(kernel);
```

Your particular syntax for how the Ninject injection should work will depend on your project (class library, web site, etc.)

### Installing EventGen

The project is on [Nuget](https://www.nuget.org/packages/DnDGen.EventGen). Install via the NuGet Package Manager.

    PM > Install-Package DnDGen.EventGen
