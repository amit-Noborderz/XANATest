
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <MobileCoreServices/MobileCoreServices.h>

// Import UniformTypeIdentifiers only if iOS 14+ is available
#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 140000
#import <UniformTypeIdentifiers/UniformTypeIdentifiers.h>
#endif

// Unity callback methods
extern "C" {
    void UnitySendMessage(const char* obj, const char* method, const char* msg);
}
static NSString *_currentGameObjectName = @"FilePickerAndUploader";
@interface FilePickerDelegate : NSObject <UIDocumentPickerDelegate>
@end

@implementation FilePickerDelegate

- (void)documentPicker:(UIDocumentPickerViewController *)controller didPickDocumentsAtURLs:(NSArray<NSURL *> *)urls {
    if (urls.count > 0) {
        NSURL *selectedURL = urls.firstObject;
        
        // Start accessing the security-scoped resource
        if ([selectedURL startAccessingSecurityScopedResource]) {
            // Copy file to app's documents directory
            NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
            NSString *documentsDirectory = [paths objectAtIndex:0];
            NSString *fileName = [selectedURL lastPathComponent];
            NSString *localPath = [documentsDirectory stringByAppendingPathComponent:fileName];
            
            NSError *error;
            NSData *fileData = [NSData dataWithContentsOfURL:selectedURL options:0 error:&error];
            
            if (fileData && !error) {
                BOOL success = [fileData writeToFile:localPath options:NSDataWritingAtomic error:&error];
                
                if (success) {
                    // Send success message to Unity
                    UnitySendMessage([_currentGameObjectName UTF8String], "OnFileSelected", [localPath UTF8String]);
                } else {
                    UnitySendMessage([_currentGameObjectName UTF8String], "OnFileSelected", "");
                }
            } else {
                UnitySendMessage([_currentGameObjectName UTF8String], "OnFileSelected", "");
            }
            
            // Stop accessing the security-scoped resource
            [selectedURL stopAccessingSecurityScopedResource];
        } else {
            UnitySendMessage([_currentGameObjectName UTF8String], "OnFileSelected", "");
        }
    }
}

- (void)documentPickerWasCancelled:(UIDocumentPickerViewController *)controller {
    UnitySendMessage([_currentGameObjectName UTF8String], "OnFilePickerCancelled", "");
}

@end

static FilePickerDelegate *_filePickerDelegate = nil;

extern "C" {
    void _PickFile(const char* ObjectName) {
        if (_filePickerDelegate == nil) {
            _filePickerDelegate = [[FilePickerDelegate alloc] init];
        }
        _currentGameObjectName = [NSString stringWithUTF8String:ObjectName];
        UIViewController *rootViewController = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
        
        UIDocumentPickerViewController *documentPicker;
        
        // Use different initialization methods based on iOS version
        if (@available(iOS 14.0, *)) {
            // iOS 14+ with UniformTypeIdentifiers
            #if __IPHONE_OS_VERSION_MAX_ALLOWED >= 140000
            NSArray<UTType *> *allowedTypes = @[UTTypeItem];
            documentPicker = [[UIDocumentPickerViewController alloc] initForOpeningContentTypes:allowedTypes];
            #endif
        } else {
            // iOS 11-13 with legacy UTI strings
            NSArray<NSString *> *allowedUTIs = @[
                @"public.item",                    // All files
                @"public.content",                 // All content
                @"public.data",                    // All data
                @"public.image",                   // Images
                @"public.movie",                   // Videos
                @"public.audio",                   // Audio
                @"com.adobe.pdf",                  // PDF
                @"public.plain-text",              // Text files
                @"public.zip-archive",             // ZIP files
                @"com.microsoft.word.doc",         // Word docs
                @"org.openxmlformats.wordprocessingml.document" // DOCX
            ];
            documentPicker = [[UIDocumentPickerViewController alloc] initWithDocumentTypes:allowedUTIs inMode:UIDocumentPickerModeImport];
        }
        
        documentPicker.delegate = _filePickerDelegate;
        documentPicker.modalPresentationStyle = UIModalPresentationFormSheet;
        
        // Set allowsMultipleSelection only if available
        if ([documentPicker respondsToSelector:@selector(setAllowsMultipleSelection:)]) {
            documentPicker.allowsMultipleSelection = NO;
        }
        
        [rootViewController presentViewController:documentPicker animated:YES completion:nil];
    }
    
    bool _IsFilePickerSupported() {
        return YES; // Document picker is available on iOS 11+
    }
}
