using InputSimulator_RV;

namespace AIRH_MAX.ClassView.GamePass
{
    class Engrane
    {        
        public static Dictionary<string, InputSimulator.Keyboard.VirtualKeyShort> keyMapping = new Dictionary<string, InputSimulator.Keyboard.VirtualKeyShort>
               {
                    { "A", InputSimulator.Keyboard.VirtualKeyShort.KEY_A },
                    { "B", InputSimulator.Keyboard.VirtualKeyShort.KEY_B },
                    { "C", InputSimulator.Keyboard.VirtualKeyShort.KEY_C },
                    { "D", InputSimulator.Keyboard.VirtualKeyShort.KEY_D },
                    { "E", InputSimulator.Keyboard.VirtualKeyShort.KEY_E },
                    { "F", InputSimulator.Keyboard.VirtualKeyShort.KEY_F },
                    { "G", InputSimulator.Keyboard.VirtualKeyShort.KEY_G },
                    { "H", InputSimulator.Keyboard.VirtualKeyShort.KEY_H },
                    { "I", InputSimulator.Keyboard.VirtualKeyShort.KEY_I },
                    { "J", InputSimulator.Keyboard.VirtualKeyShort.KEY_J },
                    { "K", InputSimulator.Keyboard.VirtualKeyShort.KEY_K },
                    { "L", InputSimulator.Keyboard.VirtualKeyShort.KEY_L },
                    { "M", InputSimulator.Keyboard.VirtualKeyShort.KEY_M },
                    { "N", InputSimulator.Keyboard.VirtualKeyShort.KEY_N },
                    { "O", InputSimulator.Keyboard.VirtualKeyShort.KEY_O },
                    { "P", InputSimulator.Keyboard.VirtualKeyShort.KEY_P },
                    { "Q", InputSimulator.Keyboard.VirtualKeyShort.KEY_Q },
                    { "R", InputSimulator.Keyboard.VirtualKeyShort.KEY_R },
                    { "S", InputSimulator.Keyboard.VirtualKeyShort.KEY_S },
                    { "T", InputSimulator.Keyboard.VirtualKeyShort.KEY_T },
                    { "U", InputSimulator.Keyboard.VirtualKeyShort.KEY_U },
                    { "V", InputSimulator.Keyboard.VirtualKeyShort.KEY_V },
                    { "W", InputSimulator.Keyboard.VirtualKeyShort.KEY_W },
                    { "X", InputSimulator.Keyboard.VirtualKeyShort.KEY_X },
                    { "Y", InputSimulator.Keyboard.VirtualKeyShort.KEY_Y },
                    { "Z", InputSimulator.Keyboard.VirtualKeyShort.KEY_Z },
                    { "D0", InputSimulator.Keyboard.VirtualKeyShort.KEY_0 },
                    { "D1", InputSimulator.Keyboard.VirtualKeyShort.KEY_1 },
                    { "D2", InputSimulator.Keyboard.VirtualKeyShort.KEY_2 },
                    { "D3", InputSimulator.Keyboard.VirtualKeyShort.KEY_3 },
                    { "D4", InputSimulator.Keyboard.VirtualKeyShort.KEY_4 },
                    { "D5", InputSimulator.Keyboard.VirtualKeyShort.KEY_5 },
                    { "D6", InputSimulator.Keyboard.VirtualKeyShort.KEY_6 },
                    { "D7", InputSimulator.Keyboard.VirtualKeyShort.KEY_7 },
                    { "D8", InputSimulator.Keyboard.VirtualKeyShort.KEY_8 },
                    { "D9", InputSimulator.Keyboard.VirtualKeyShort.KEY_9 },
                    { "F1", InputSimulator.Keyboard.VirtualKeyShort.F1 },
                    { "F2", InputSimulator.Keyboard.VirtualKeyShort.F2 },
                    { "F3", InputSimulator.Keyboard.VirtualKeyShort.F3 },
                    { "F4", InputSimulator.Keyboard.VirtualKeyShort.F4 },
                    { "F5", InputSimulator.Keyboard.VirtualKeyShort.F5 },
                    { "F6", InputSimulator.Keyboard.VirtualKeyShort.F6 },
                    { "F7", InputSimulator.Keyboard.VirtualKeyShort.F7 },
                    { "F8", InputSimulator.Keyboard.VirtualKeyShort.F8 },
                    { "F9", InputSimulator.Keyboard.VirtualKeyShort.F9 },
                    { "F10", InputSimulator.Keyboard.VirtualKeyShort.F10 },
                    { "F11", InputSimulator.Keyboard.VirtualKeyShort.F11 },
                    { "F12", InputSimulator.Keyboard.VirtualKeyShort.F12 },
                    { "NumPad0", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD0 },
                    { "NumPad1", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD1 },
                    { "NumPad2", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD2 },
                    { "NumPad3", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD3 },
                    { "NumPad4", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD4 },
                    { "NumPad5", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD5 },
                    { "NumPad6", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD6 },
                    { "NumPad7", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD7 },
                    { "NumPad8", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD8 },
                    { "NumPad9", InputSimulator.Keyboard.VirtualKeyShort.NUMPAD9 },
                    { "Divide", InputSimulator.Keyboard.VirtualKeyShort.DIVIDE },
                    { "Multiply", InputSimulator.Keyboard.VirtualKeyShort.MULTIPLY },
                    { "Subtract", InputSimulator.Keyboard.VirtualKeyShort.OEM_MINUS },
                    { "Add", InputSimulator.Keyboard.VirtualKeyShort.OEM_PLUS },
                    { "Up", InputSimulator.Keyboard.VirtualKeyShort.UP },
                    { "Down", InputSimulator.Keyboard.VirtualKeyShort.DOWN },
                    { "Left", InputSimulator.Keyboard.VirtualKeyShort.LEFT },
                    { "Right", InputSimulator.Keyboard.VirtualKeyShort.RIGHT },
                    { "Decimal", InputSimulator.Keyboard.VirtualKeyShort.DECIMAL },
                    { "Escape", InputSimulator.Keyboard.VirtualKeyShort.ESCAPE },
                    { "Insert", InputSimulator.Keyboard.VirtualKeyShort.INSERT },
                    { "Delete", InputSimulator.Keyboard.VirtualKeyShort.DELETE },
                    { "Home", InputSimulator.Keyboard.VirtualKeyShort.HOME },
                    { "End", InputSimulator.Keyboard.VirtualKeyShort.END },
                    { "Next", InputSimulator.Keyboard.VirtualKeyShort.NEXT },
                    { "Print", InputSimulator.Keyboard.VirtualKeyShort.PRINT },
                    { "Tab", InputSimulator.Keyboard.VirtualKeyShort.TAB },
                    { "Capital", InputSimulator.Keyboard.VirtualKeyShort.CAPITAL },
                    { "LeftShift", InputSimulator.Keyboard.VirtualKeyShort.LSHIFT },
                    { "LeftControl", InputSimulator.Keyboard.VirtualKeyShort.LCONTROL },
                    { "LWin", InputSimulator.Keyboard.VirtualKeyShort.LWIN },
                    { "Space", InputSimulator.Keyboard.VirtualKeyShort.SPACE },
                    { "LeftAlt", InputSimulator.Keyboard.VirtualKeyShort.LMENU },
                    { "RightAlt", InputSimulator.Keyboard.VirtualKeyShort.RMENU },
                    { "RWin", InputSimulator.Keyboard.VirtualKeyShort.RWIN },
                    { "Apps", InputSimulator.Keyboard.VirtualKeyShort.APPS },
                    { "RightControl", InputSimulator.Keyboard.VirtualKeyShort.RCONTROL },
                    { "RightShift", InputSimulator.Keyboard.VirtualKeyShort.RSHIFT },
                    { "º", InputSimulator.Keyboard.VirtualKeyShort.OEM_5 },
                    { ",", InputSimulator.Keyboard.VirtualKeyShort.OEM_COMMA },
                    { ".", InputSimulator.Keyboard.VirtualKeyShort.OEM_PERIOD },
                    { "-", InputSimulator.Keyboard.VirtualKeyShort.OEM_MINUS },
                    { "+", InputSimulator.Keyboard.VirtualKeyShort.OEM_PLUS }
               };

        static Dictionary<string, int> Contadores = new Dictionary<string, int>
        {
            { "A", 0 }, { "B", 0 }, { "C", 0 }, { "D", 0 }, { "E", 0 }, { "F", 0 }, { "G", 0 }, { "H", 0 },
            { "I", 0 }, { "J", 0 }, { "K", 0 }, { "L", 0 }, { "M", 0 }, { "N", 0 }, { "O", 0 }, { "P", 0 },
            { "Q", 0 }, { "R", 0 }, { "S", 0 }, { "T", 0 }, { "U", 0 }, { "V", 0 }, { "W", 0 }, { "X", 0 },
            { "Y", 0 }, { "Z", 0 }, { "F1", 0 }, { "F2", 0 }, { "F3", 0 }, { "F4", 0 }, { "F5", 0 }, { "F6", 0 },
            { "F7", 0 }, { "F8", 0 }, { "F9", 0 }, { "F10", 0 }, { "F11", 0 }, { "F12", 0 }, { "VkNum00i", 0 },
            { "VkNum01i", 0 }, { "VkNum02i", 0 }, { "VkNum03i", 0 }, { "VkNum04i", 0 }, { "VkNum05i", 0 },
            { "VkNum06i", 0 }, { "VkNum07i", 0 }, { "VkNum08i", 0 }, { "VkNum09i", 0 }, { "NumPad00i", 0 },
            { "NumPad01i", 0 }, { "NumPad02i", 0 }, { "NumPad03i", 0 }, { "NumPad04i", 0 }, { "NumPad05i", 0 },
            { "NumPad06i", 0 }, { "NumPad07i", 0 }, { "NumPad08i", 0 }, { "NumPad09i", 0 }, { "Suma", 0 },
            { "Menos", 0 }, { "Divisor", 0 }, { "Multiplicar", 0 }, { "FlechaUp", 0 }, { "FlechaDown", 0 },
            { "FlechaLeft", 0 }, { "FlechaRight", 0 }, { "Combo01", 0 }, { "Combo02", 0 }, { "Combo03", 0 },
            { "Combo04", 0 }, { "Combo05", 0 }, { "Combo06", 0 }, { "Combo07", 0 }, { "Combo08", 0 },
            { "Combo09", 0 }, { "Combo10", 0 }, { "Combo11", 0 }, { "Combo12", 0 }, { "Combo13", 0 },
            { "Combo14", 0 }, { "Combo15", 0 }, { "Combo16", 0 }, { "Combo17", 0 }, { "Combo18", 0 },
            { "Combo19", 0 }, { "Combo20", 0 }, { "Combo21", 0 }, { "Combo22", 0 }, { "Combo23", 0 },
            { "Combo24", 0 }, { "ControlL", 0 }, { "ControlR", 0 }, { "Alt", 0 }, { "Enter", 0 },
            { "ShiftL", 0 }, { "ShiftR", 0 }, { "Espacio", 0 }, { "Pulsar01", 0 }
        };

        public static void ResetContador()
        {
            foreach (var key in Contadores.Keys.ToList())
            {
                Contadores[key] = 0;
            }
        }

        public static string GamerCommand(string speech)
        {
            Autoclick.Comandos(speech);
            Secuencia.Comandos(speech);
            return Teclado.Comandos(speech);
        }
    }
}
