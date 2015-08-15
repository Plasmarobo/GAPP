#include "utils.h"

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
}