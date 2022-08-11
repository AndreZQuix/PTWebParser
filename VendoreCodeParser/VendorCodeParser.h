#pragma once
#include <string>
#include <comdef.h>
#include <iterator>

extern "C" __declspec(dllexport) BSTR ParseVendorCode(const wchar_t* s);