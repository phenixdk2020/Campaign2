# Version og test

## Du er ikke på atlas endnu

Hvis Game-view øverst siger `v00.00.18 UNIFIED DENMARK` og Stockholm sidder på Aarhus, kører den gamle scene. `TheaterAtlasBootstrap` er ikke i Play.

## Flyt versionen længere ned — nu

1. Kopiér `CampaignVersionHud.cs` til `Assets/Scripts/`
2. Add Component på et tomt objekt i den scene du allerede bruger
3. Play

Linjen tegnes ved **y = 72** (under Unity-værktøjslinjen). Feltet `offsetFromTop` kan skrues op hvis den stadig dækker Pause-knapperne.

- Gul: `v00.00.18 UNIFIED — atlas kører IKKE`
- Pergament: `v00.00.03 ATLAS`

## Atlas (når du vil se den nye stil)

1. `rings.txt` i `Assets/Resources/CampaignMap/`
2. `TheaterAtlasBootstrap.cs` på et objekt i scenen
3. Slå UNIFIED-meshen fra
4. Play — pergamentkort + linjen skifter til ATLAS
