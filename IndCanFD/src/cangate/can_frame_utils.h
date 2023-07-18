#ifndef CAN_FRAME_UTILS_H
#define CAN_FRAME_UTILS_H

#include <linux/can.h>

void fill_can_frame(struct can_frame *frame, char *data);

#endif
