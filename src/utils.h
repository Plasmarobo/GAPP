#ifndef UTILS_H
#define UTILS_H
#include <string>
#include <fstream>
#include <iostream>

class Logger
{
protected:
	static std::fstream m_log_file;
public:
	Logger(std::string filename);
	~Logger();
	static void RaiseError(std::string tag, std::string message);
};

#endif