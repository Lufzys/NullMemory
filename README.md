# Advanced Memory class

Advanced Memory Read/Write class

  - [x] Memory Read/Write
  - [x] WorldToScreen function
  - [x] FindDMAAddy function
  - [x] Advanced Signature Scan function
  - [x] Module Object
  - [x] others ...

## Example usage

```csharp

Memorys.Initialize(p[0].Id);
Memorys.Module client = new Memorys.Module(p[0], "client.dll");
Memorys.Module engine = new Memorys.Module(p[0], "engine.dll");

strClient.Text = client.ToString();
StrEngine.Text = engine.ToString();
int clientstate = Memorys.FindPattern(engine, "A1 ? ? ? ? 33 D2 6A 00 6A 00 33 C9 89 B0", 1, 0, true);
/*
                 
{
   "name": "dwClientState",
   "extra": 0,
   "relative": true,
   "module": "engine.dll",
   "offsets": [
      1
    ],
    "pattern": "A1 ? ? ? ? 33 D2 6A 00 6A 00 33 C9 89 B0"
},
                 
*/
int clientstate_state = Memorys.FindPattern(engine, "83 B8 ? ? ? ? ? 0F 94 C0 C3", 2, 0, false);
/*

{
   "name": "dwClientState_State",
   "extra": 0,
   "relative": false,
   "module": "engine.dll",
   "offsets": [
      2
    ],
    "pattern": "83 B8 ? ? ? ? ? 0F 94 C0 C3"
},

*/

int EngineBase = Memorys.ReadMemory<int>(engine.Address + clientstate);
strResult.Text = "dwClientState => 0x" + clientstate.ToString("X") + Environment.NewLine +
                 "dwClientState_State => 0x" + (clientstate_state).ToString("X") + Environment.NewLine +
                 "dwState => " + Memorys.ReadMemory<int>(EngineBase + clientstate_state).ToString();

```

## Example usage - Output
![alt text](https://github.com/Lufzy/Advanced-Memory/blob/master/example_output.PNG?raw=true)

  - [Signature Scan working correctly]
![alt text](https://github.com/Lufzy/Advanced-Memory/blob/master/memorys_example.PNG?raw=true)

## License
[MIT](https://choosealicense.com/licenses/mit/)
