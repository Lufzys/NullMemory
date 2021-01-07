using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NullMemory.Demo
{
    public partial class MainForm : Form
    {
        NullMemory nullMem = new NullMemory("csgo");
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
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

                int EngineBase = new NullMemory.Kernel(nullMem).Read<int>((IntPtr)engine.Address + clientstate);
                string resultText = "dwClientState => 0x" + clientstate.ToString("X") + Environment.NewLine +
                                 "dwClientState_State => 0x" + (clientstate_state).ToString("X") + Environment.NewLine +
                                 "dwState => " + new NullMemory.Kernel(nullMem).Read<int>((IntPtr)EngineBase + clientstate_state).ToString();
                MessageBox.Show(resultText, "Null Memory", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
