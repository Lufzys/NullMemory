# NULL Memory

Advanced Memory Read/Write class

  - [x] Memory Read/Write
  - [x] Faster(NtVirtualMemory) Memory Read/Write
  - [x] WorldToScreen function
  - [x] FindDMAAddy function
  - [x] Advanced Signature Scan function
  - [x] Module Object
  - [x] others ...
  
  NtReadVirtualMemory/NtWriteVirtualMemory vs ReadProcessMemory/WriteProcessMemory                              :
  https://guidedhacking.com/threads/make-your-external-cheats-super-fast-read-writeprocessmemory-vs-ntread-ntwritevirtualmemory.15194/ |
  https://www.unknowncheats.me/forum/general-programming-and-reversing/285926-writeprocessmemory-lot-slower-ntwritevirtualmemory-windows-10-a.html

## Example usage

```csharp
            Enums.InitializeResult Result = nullMem.Initialize();
            MessageBox.Show(Result.ToString(), "Null Memory", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if(Result == Enums.InitializeResult.Succesfully)
            {
                NullMemory.Module client = new NullMemory.Module(nullMem.NullProcess, "client.dll");
                NullMemory.Module engine = new NullMemory.Module(nullMem.NullProcess, "engine.dll");

                string strClient = client.ToString();
                string StrEngine = engine.ToString();
                MessageBox.Show("Client => " + strClient + Environment.NewLine + "Engine => " + StrEngine, "Null Memory", MessageBoxButtons.OK, MessageBoxIcon.Information);
                int clientstate = nullMem.FindPattern(engine, "A1 ? ? ? ? 33 D2 6A 00 6A 00 33 C9 89 B0", 1, 0, true);
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
                int clientstate_state = nullMem.FindPattern(engine, "83 B8 ? ? ? ? ? 0F 94 C0 C3", 2, 0, false);
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

                int EngineBase = nullMem.Read<int>(engine.Address + clientstate);
                string resultText = "dwClientState => 0x" + clientstate.ToString("X") + Environment.NewLine +
                                 "dwClientState_State => 0x" + (clientstate_state).ToString("X") + Environment.NewLine +
                                 "dwState => " + nullMem.Read<int>(EngineBase + clientstate_state).ToString();
                MessageBox.Show(resultText, "Null Memory", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

```

## Example usage - Output | Old
![alt text](https://github.com/Lufzy/Advanced-Memory/blob/master/example_output.PNG?raw=true)

  - [Signature Scan working correctly] | Old
![alt text](https://github.com/Lufzy/Advanced-Memory/blob/master/memorys_example.PNG?raw=true)

## License
[MIT](https://choosealicense.com/licenses/mit/)
