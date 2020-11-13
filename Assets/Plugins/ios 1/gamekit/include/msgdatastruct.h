//
//  msgdatastruct.h
//  gamekit
//
//  Created by chenjianquan on 16/6/14.
//  Copyright © 2016年 feiyin. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface MessageTable : NSObject

@property (strong,readonly) NSMutableDictionary* mDictioary;

-(id) init;

-(void) setValue:(NSString*) value forKey:(NSString*) key;

-(NSString*) getValue:(NSString*) key forDeafult:(NSString*) value;

-(void) setTable:(NSString*) table;

-(NSString*) serialize;

@end
