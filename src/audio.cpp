#ifndef AUDIO_H
#define AUDIO_H

enum Sounds {
	SILENCE = 0,
	QUADRANGLE_SWEEP_ENV,
	QUADRANGLE_ENV,
	VOL_WAVE,
	NOISE_ENV
};


class Audio
{
protected:
	unsigned int HzToGb(unsigned int hz) { return (2048 - (131072 / hz)); }
	unsigned int GbToHz(unsigned int gb) { return (131072 / (2048 - gb)); }

};
#endif