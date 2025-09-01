
#import <UIKit/UIKit.h>

extern void CopyToClipboardiOS(const char* text);

void CopyToClipboardiOS(const char* text)
{
    NSString *textToCopy = [NSString stringWithUTF8String:text];
    UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
    pasteboard.string = textToCopy;
}
