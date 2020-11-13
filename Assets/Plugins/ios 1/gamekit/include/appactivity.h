//
//  appactivity.h
//  gamekit
//
//  Created by chenjianquan on 12/02/2018.
//  Copyright © 2018 feiyin. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>
#import "msgdatastruct.h"

@interface AppActivity : NSObject
{

}


-(id) init;

-(void) start;

-(BOOL) callMethod :(NSString*) methodName requestParam:(NSString*) param;

-(void) showSplashView;

-(void) onNativeInfo;

-(void) onCustomIcon:(UIViewController*)inViewController;

-(void) onCustomImage:(UIViewController*)inViewController;

-(void) pasteBoard:(NSString*)Text;

+(id) getInstance;

+(void) SendUnityMessage:(NSString*) eventId forMessage:(NSString*) message;

+(void) SendUnityTable:(NSString*) eventId forMessage:(MessageTable*) table;

@end
