file_bin_write_byte(argument0, argument1 & $ff)
file_bin_write_byte(argument0, (argument1 & $ff00) >> 8)
file_bin_write_byte(argument0, (argument1 & $ff0000) >> 16)
file_bin_write_byte(argument0, (argument1 & $ff000000) >> 24)
