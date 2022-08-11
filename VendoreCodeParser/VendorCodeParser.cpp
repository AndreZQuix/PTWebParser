#include "pch.h"
#include "VendorCodeParser.h"
#include <locale>
#include <codecvt>

bool isVendorCodeChar(wchar_t c)
{
    static const std::wstring venCodeChar = L"~;/?:x-¹(),.=";
    return (!iswalnum(c) || find(venCodeChar.begin(), venCodeChar.end(), c) != venCodeChar.end());
}

wchar_t getChar(std::wstring::iterator iter)
{
    return *iter;
}

std::wstring::iterator venCodeBeg(std::wstring::iterator b, std::wstring::iterator e)
{
    typedef std::wstring::iterator iter;
    iter i = b;
    while (i != e)
    {
        if (iswupper(getChar(i)) || iswdigit(getChar(i)))
        {
            iter next = std::next(i);
            if (next != e && (iswupper(getChar(next)) || iswdigit(getChar(next)) || isVendorCodeChar(getChar(next))))
                return i;
        }
        ++i;
    }
    return i;
}

std::wstring::iterator venCodeEnd(std::wstring::iterator b, std::wstring::iterator e)
{
    return find_if(b, e, iswspace);
}

extern "C" __declspec(dllexport) BSTR ParseVendorCode(const wchar_t* s)
{
    std::wstring name(s);
    typedef std::wstring::iterator iter;
    iter b = name.begin(), e = name.end();
    setlocale(LC_ALL, "");
    while (b != e)
    {
        b = venCodeBeg(b, e);
        if (b != e)
        {
            //iter after = venCodeEnd(b, e);
            //int codeSize = std::distance(b, after);
            //int wcharsCount = MultiByteToWideChar(CP_ACP, 0, &*b, codeSize, NULL, 0);
            //std::wstring wstr;
            //wstr.resize(wcharsCount);
            //MultiByteToWideChar(CP_ACP, 0, &*b, codeSize, &wstr[0], wcharsCount);

            iter after = venCodeEnd(b, e);    
            std::wstring wstr(b, after);
            return SysAllocString(wstr.c_str());
        }
    }
    return SysAllocString(L"");
}