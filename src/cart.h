#ifndef CART_H
#define CART_H
#include <string>

enum RomSize
{
	Size32k = 0,
	Size64k,
	Size128k,
	Size256k,
	Size512k,
	Size1024k,
	Size2048k,
	Size4096k,
};

enum RamSize
{
	Size0k = 0,
	Size2k,
	Size8k,
	Size32k,
};

class Cart
{
protected:
	unsigned char title[16];
	unsigned char new_licensee[2];
	unsigned char sgb;
	unsigned char type;
	unsigned char rom_size;
	unsigned char ram_size;
	unsigned char country;
	unsigned char licensee;
	unsigned char header_check;
	unsigned short checksum;

	unsigned char *rom;
	unsigned char *ram;
public:
	Cart();
	~Cart();
	void Load(std::string gb_file);
	unsigned char Read(unsigned short addr);
	void Write(unsigned short addr);


};

#endif