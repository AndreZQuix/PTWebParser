#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <Windows.h>

using namespace std;

namespace VendorCodeParser
{
    struct Product
    {
        string vendorCode;
        string compCode;
        string name;
        double price;
    };

    bool isVendorCodeChar(unsigned char c)
    {
        static const string venCodeChar = "~;/?:x-№(),.=";
        return (!isalnum(c) || find(venCodeChar.begin(), venCodeChar.end(), c) != venCodeChar.end());
    }

    unsigned char getChar(string::iterator iter)
    {
        return (unsigned char)*iter;
    }

    string::iterator venCodeBeg(string::iterator b, string::iterator e)
    {
        typedef string::iterator iter;
        iter i = b;
        while (i != e)
        {
            if (isupper(getChar(i)) || isdigit(getChar(i))) // пункт первый
            {
                ++i;
                if (isupper(getChar(i)) || isdigit(getChar(i)) || isVendorCodeChar(getChar(i)))    // пункт второй (без учета пробела)
                    return --i;
            }
            ++i;
        }
        return i;
    }

    string::iterator venCodeEnd(string::iterator b, string::iterator e)
    {
        return find_if(b, e, isspace);
    }

    void parseVendoreCode(Product& p)
    {
        typedef string::iterator iter;
        iter b = p.name.begin(), e = p.name.end();
        while (b != e)
        {
            b = venCodeBeg(b, e);
            if (b != e)
            {
                iter after = venCodeEnd(b, e);
                p.vendorCode = string(b, after);
                return;
            }
        }
    }

    void fillProducts(ifstream& ifs, ofstream& ofs)
    {
        if (ifs.is_open() && ofs.is_open())
        {
            string skipFirstString;
            getline(ifs, skipFirstString);
            int id = 1;
            while (!ifs.eof())
            {
                Product product;
                getline(ifs, product.name, '|');

                if (product.name.empty())
                    break;

                parseVendoreCode(product);
                getline(ifs, product.compCode);
                string price;
                getline(ifs, price);
                product.price = stod(price);

                if (product.vendorCode.size() <= 3)
                    continue;

                ofs << "ID:" << id << '\n'
                    << "Name:" << product.name << '\n'
                    << "CompID:" << product.compCode << '\n'
                    << "Price:" << product.price << '\n'
                    << "VendorCode:" << product.vendorCode << endl;

                id++;
            }
        }
        ifs.close();
    }

    void initializeStreams(ifstream& ifs, ofstream& ofs)
    {
        ifstream config("$(ProjectDir)..\config.ini");
        if (config.is_open())
        {
            string fileName;
            getline(config, fileName);
            ifs.open(fileName);

            fileName.clear();
            getline(config, fileName);
            ofs.open(fileName, ios::app);

            if (ifs.is_open() && ofs.is_open())
            {
                cout << "Данные config.ini считаны, потоки r/w открыты\n\n";
            }
            else
            {
                cout << "Ошибка открытия потока\n\n";
            }
        }
    }

    int main()
    {
        setlocale(LC_ALL, "Russian");
        SetConsoleCP(1251);
        SetConsoleOutputCP(1251);

        ifstream ifs;
        ofstream ofs;
        initializeStreams(ifs, ofs);
        fillProducts(ifs, ofs);

        return 0;
    }
}
