# UnityMMO_Setup

## Set up Unity

Go to cloud.unity.com
+ Set up an organization if you have not already

#### Add Packages

Package Manger -> Netcode for GameObjects & Multiplayer Services

#### Configure Project Settings

Edit -> Project Settings -> Services
select organization, create new cloud project, link project


## Create a Player prefab

+ Add Network Object, Network Transform, Network RigidBody

## Create a Network Manager

+ Add Network Manager component and choose Unity Transport
	+ This will automatically add Unity Transport component
+ Add the player prefab to Default Player Prefab in Network Manager



## Where we left off...
Looking at RelayInitializer.cs, RelayClient.cs, and RelayHost.cs to see how to create a one-size-fits-all approach to setting up a Relay networked Unity game.
