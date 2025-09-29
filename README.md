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

## Start a multiplayer game

+ Host via RelayManager "Relay Host" component by clicking the three dots
+ Join via RelayManager "Relay Client" component by clicking the three dots...

## Where we left off...

+ add a player steps on item to increase network variable
+ get player names working

## notes
+ 1) Awake, 2) OnNetworkSpawn, 3) Start