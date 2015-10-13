#include "utils.h"
#include <fstream>
#include <iostream>

#ifdef _DEBUG
#include <assert.h>
#include <intrin.h>
#endif

std::fstream Logger::m_log_file;

Logger::Logger(std::string filename)
{
	m_log_file.open(filename, std::ios_base::out);
	if (!m_log_file.is_open())
	{
		std::cerr << "Could not open logfile" << std::endl;
	}
}
Logger::~Logger()
{
	m_log_file.close();
}

void Logger::RaiseError(std::string tag, std::string message)
{
	std::string entry = "ERROR   " + tag + ": " + message;
	std::cerr << entry << std::endl;
	if (m_log_file.is_open())
	{
		m_log_file << entry << std::endl;
	}
#ifdef _DEBUG
	__debugbreak();
#endif
}

void Logger::PrintInfo(std::string tag, std::string message)
{
	std::string entry = "INFO   " + tag + ": " + message;
	std::cout << entry << std::endl;
	if (m_log_file.is_open())
	{
		m_log_file << entry << std::endl;
	}
}