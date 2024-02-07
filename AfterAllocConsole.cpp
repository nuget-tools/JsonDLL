#include <stdio.h>

extern "C"
__declspec(dllexport)
void AfterAllocConsole()
{
   freopen("CONIN$", "r", stdin);
   freopen("CONOUT$", "w", stdout);
   freopen("CONERR$", "w", stdout);
}
