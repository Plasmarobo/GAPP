#ifndef UTILS_H
#define UTILS_H

#include <string>


class Logger
{
protected:
	static std::fstream m_log_file;
public:
	Logger(std::string filename);
	~Logger();
	static void RaiseError(std::string tag, std::string message);
	static void PrintInfo(std::string tag, std::string message);
};

#endif